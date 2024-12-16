using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public static class UltraFastLinkExtractor
{
    private const int BufferSize = 1028 * 256; // 256KB buffer for better streaming
    private const int UrlBufferSize = 32768; // 32KB for URL storage
    private const int MaxUrlLength = 2048;
    private const uint HrefPattern = 0x66657268; // 'href' in reverse
    
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;
    private static readonly Vector256<byte> QuoteChars = Vector256.Create(
        (byte)'\'', (byte)'\"', (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"', (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"', (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"', (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"', (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"', (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"', (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"', (byte)'\'', (byte)'\"'
    );

    // Pre-computed lookup tables
    private static readonly byte[] ValidUrlChars = CreateValidUrlChars();
    private static readonly int[] HexLookup = CreateHexLookup();
    
    // Reusable buffers per thread
    private static readonly ThreadLocal<byte[]> UrlBuffer = new(() => new byte[UrlBufferSize]);
    private static readonly ThreadLocal<int[]> UrlLengths = new(() => new int[UrlBufferSize / MaxUrlLength]);
    
    // Version 1: 32-bit packed (4 bytes)
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private readonly struct QuotePosition 
    {
        private readonly uint _data;
    
        // 15 bits each = positions up to 32KB
        public readonly int Start => (int)(_data & 0x7FFF);
        public readonly int Length => (int)((_data >> 15) & 0x7FFF);
        public readonly bool IsValid => (_data & 0x40000000) != 0;
    
        public QuotePosition(int start, int length, bool isValid)
        {
            _data = ((uint)(start & 0x7FFF)) |
                    ((uint)(length & 0x7FFF) << 15) |
                    (isValid ? 0x40000000u : 0);
        }
    }
    
    private static byte[] CreateValidUrlChars()
    {
        var valid = new byte[128];
        // Standard URL characters
        for (int i = '0'; i <= '9'; i++) valid[i] = 1;
        for (int i = 'A'; i <= 'Z'; i++) valid[i] = 1;
        for (int i = 'a'; i <= 'z'; i++) valid[i] = 1;
        // Special URL characters
        string special = "-._~:/?#[]@!$&'()*+,;=%";
        foreach (char c in special) valid[c] = 1;
        return valid;
    }
    
    private static int[] CreateHexLookup()
    {
        var lookup = new int[128];
        for (int i = 0; i < lookup.Length; i++) lookup[i] = -1;
        for (int i = '0'; i <= '9'; i++) lookup[i] = i - '0';
        for (int i = 'a'; i <= 'f'; i++) lookup[i] = 10 + (i - 'a');
        for (int i = 'A'; i <= 'F'; i++) lookup[i] = 10 + (i - 'A');
        return lookup;
    }

    public static async Task<List<string>> ExtractHrefsAsync(Stream responseStream)
    {
        var foundLinks = new List<string>();
        byte[] buffer = BufferPool.Rent(BufferSize);
        int remainingBytes = 0;
        
        // Get thread-local buffers
        byte[] urlBuffer = UrlBuffer.Value;
        int[] urlLengths = UrlLengths.Value;
        int urlCount = 0;
        
        try
        {
            while (true)
            {
                int bytesRead = await responseStream.ReadAsync(
                    buffer.AsMemory(remainingBytes, BufferSize - remainingBytes)).ConfigureAwait(false);
                    
                if (bytesRead == 0) break;
                bytesRead += remainingBytes;
                
                ProcessBufferOptimized(buffer, bytesRead, ref remainingBytes, urlBuffer, urlLengths, ref urlCount);
                
                if (remainingBytes > 0 && remainingBytes < bytesRead)
                {
                    Buffer.BlockCopy(buffer, bytesRead - remainingBytes, buffer, 0, remainingBytes);
                }
                
                // Convert accumulated URLs to strings
                if (urlCount > 0)
                {
                    ConvertUrlsToStrings(urlBuffer, urlLengths, urlCount, foundLinks);
                    urlCount = 0;
                }
            }
            
            return foundLinks;
        }
        finally
        {
            BufferPool.Return(buffer);
        }
    }

    private static unsafe void ProcessBufferOptimized(byte[] buffer, int bytesRead, ref int remainingBytes,
        byte[] urlBuffer, int[] urlLengths, ref int urlCount)
    {
        fixed (byte* bufPtr = buffer)
        fixed (byte* urlBufPtr = urlBuffer)
        {
            int position = 0;
            int safeLength = bytesRead - 4;

            while (position < safeLength)
            {
                int hrefPos = FindNextHrefOptimized(bufPtr + position, bytesRead - position);
                if (hrefPos == -1)
                {
                    remainingBytes = Math.Min(4, bytesRead - position);
                    return;
                }

                position += hrefPos;
                byte* currentPtr = bufPtr + position;
                byte* endPtr = bufPtr + bytesRead;
                
                // Find quote and extract URL
                var quotePos = FindQuoteOptimized(currentPtr, (int)(endPtr - currentPtr));
                if (quotePos.IsValid)
                {
                    byte* urlStart = currentPtr + quotePos.Start;
                    int urlLength = quotePos.Length;
                    
                    if (urlLength > 0 && urlLength < MaxUrlLength && IsValidUrl(urlStart, urlLength))
                    {
                        // Process URL encoding and copy to buffer
                        int decodedLength = ProcessUrl(urlStart, urlLength, urlBufPtr + urlCount * MaxUrlLength);
                        if (decodedLength > 0)
                        {
                            urlLengths[urlCount++] = decodedLength;
                            
                            // Check if we need to process accumulated URLs
                            if (urlCount >= urlLengths.Length)
                            {
                                return;
                            }
                        }
                    }
                    
                    position += quotePos.Start + quotePos.Length;
                }
                else
                {
                    position += 4;
                }
                
                remainingBytes = Math.Min(4, bytesRead - position);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe bool IsValidUrl(byte* url, int length)
    {
        if (Avx2.IsSupported && length >= 32)
        {
            fixed (byte* validPtr = ValidUrlChars)
            {
                int i = 0;
                while (i <= length - 32)
                {
                    var chunk = Avx2.LoadVector256(url + i);
                    if (Avx2.MoveMask(chunk) != 0) return false;
                    
                    for (int j = 0; j < 32; j++)
                    {
                        if (validPtr[url[i + j]] == 0) return false;
                    }
                    i += 32;
                }
                
                for (; i < length; i++)
                {
                    byte c = url[i];
                    if (c >= 128 || ValidUrlChars[c] == 0) return false;
                }
            }
            return true;
        }
        
        fixed (byte* validPtr = ValidUrlChars)
        {
            for (int i = 0; i < length; i++)
            {
                byte c = url[i];
                if (c >= 128 || validPtr[c] == 0) return false;
            }
        }
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int ProcessUrl(byte* source, int length, byte* destination)
    {
        int writePos = 0;
        fixed (int* hexPtr = HexLookup)
        {
            for (int i = 0; i < length && writePos < MaxUrlLength - 1; i++)
            {
                byte c = source[i];
                
                if (c == '%' && i + 2 < length)
                {
                    byte h1 = source[i + 1];
                    byte h2 = source[i + 2];
                    
                    if (h1 < 128 && h2 < 128)
                    {
                        int hex1 = hexPtr[h1];
                        int hex2 = hexPtr[h2];
                        
                        if (hex1 >= 0 && hex2 >= 0)
                        {
                            destination[writePos++] = (byte)((hex1 << 4) | hex2);
                            i += 2;
                            continue;
                        }
                    }
                }
                
                destination[writePos++] = c;
            }
        }
        return writePos;
    }

    private static void ConvertUrlsToStrings(byte[] urlBuffer, int[] urlLengths, int count, List<string> output)
    {
        for (int i = 0; i < count; i++)
        {
            int length = urlLengths[i];
            if (length > 0 && length < MaxUrlLength)
            {
                output.Add(Encoding.ASCII.GetString(urlBuffer, i * MaxUrlLength, length));
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int FindNextHrefOptimized(byte* buffer, int length)
    {
        if (Avx2.IsSupported && length >= 32)
        {
            int i = 0;
            while (i <= length - 32)
            {
                var data = Avx2.LoadVector256(buffer + i);
                var lower = Avx2.Or(data, Vector256.Create((byte)0x20));
                var matches = Avx2.CompareEqual(lower, Vector256.Create((byte)'h'));
                int mask = Avx2.MoveMask(matches);
                
                while (mask != 0)
                {
                    int pos = i + BitOperations.TrailingZeroCount(mask);
                    if (pos <= length - 4)
                    {
                        uint pattern = *(uint*)(buffer + pos) | 0x20202020;
                        if (pattern == HrefPattern)
                        {
                            return pos;
                        }
                    }
                    mask &= mask - 1;
                }
                i += 32;
            }
        }
        
        for (int i = 0; i <= length - 4; i++)
        {
            uint word = *(uint*)(buffer + i) | 0x20202020;
            if (word == HrefPattern)
            {
                return i;
            }
        }
        
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe QuotePosition FindQuoteOptimized(byte* buffer, int length)
    {
        if (Avx2.IsSupported && length >= 32)
        {
            int i = 0;
            // Skip to first quote
            while (i <= length - 32)
            {
                var data = Avx2.LoadVector256(buffer + i);
                var matches = Avx2.CompareEqual(data, QuoteChars);
                int mask = Avx2.MoveMask(matches);
                
                if (mask != 0)
                {
                    int quotePos = BitOperations.TrailingZeroCount(mask);
                    byte quote = buffer[i + quotePos];
                    i += quotePos + 1;
                    
                    // Find matching quote
                    while (i <= length - 32)
                    {
                        data = Avx2.LoadVector256(buffer + i);
                        matches = Avx2.CompareEqual(data, Vector256.Create(quote));
                        mask = Avx2.MoveMask(matches);
                        
                        if (mask != 0)
                        {
                            int endPos = BitOperations.TrailingZeroCount(mask);
                            return new QuotePosition(quotePos + 1, i + endPos - (quotePos + 1), true);
                        }
                        i += 32;
                    }
                    
                    // Check remaining bytes
                    while (i < length)
                    {
                        if (buffer[i] == quote)
                        {
                            return new QuotePosition(quotePos + 1, i - (quotePos + 1), true);
                        }
                        i++;
                    }
                }
                i += 32;
            }
        }
        
        return new QuotePosition(0, 0, false);
    }
}