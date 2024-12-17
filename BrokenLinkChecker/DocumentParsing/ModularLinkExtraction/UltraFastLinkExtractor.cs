using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public static class UltraFastLinkExtractor
{
    private const int BoundaryOverlap = 2048; // MaxUrlLength

    private const int BufferSize = 1024 * 256; // 524,288 bytes

    // Aligned power of 2 for better memory allocation
    private const int UrlBufferSize = 32 * 1024; // 32,768 bytes
    private const int MaxUrlLength = 2048;
    private const uint HrefPattern = 0x66657268; // 'href' in reverse

    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;

    // Pre-computed lookup tables
    private static readonly byte[] ValidUrlChars = CreateValidUrlChars();
    private static readonly int[] HexLookup = CreateHexLookup();
    
    private static readonly Vector256<byte> LowerMask = Vector256.Create((byte)0x20);
    private static readonly Vector256<byte> HChar = Vector256.Create((byte)'h');

    // Reusable buffers per thread
    private static readonly ThreadLocal<byte[]> UrlBuffer = new(() => new byte[UrlBufferSize]);
    private static readonly ThreadLocal<int[]> UrlLengths = new(() => new int[UrlBufferSize / MaxUrlLength]);

    private static byte[] CreateValidUrlChars()
    {
        var valid = new byte[128];
        for (int i = '0'; i <= '9'; i++) valid[i] = 1;
        for (int i = 'A'; i <= 'Z'; i++) valid[i] = 1;
        for (int i = 'a'; i <= 'z'; i++) valid[i] = 1;
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

    public static async ValueTask<List<string>> ExtractHrefsAsync(
        Stream responseStream,
        CancellationToken cancellationToken = default)
    {
        var foundLinks = new List<string>(1028);
        byte[] buffer = BufferPool.Rent(BufferSize + BoundaryOverlap);

        try
        {
            int remainingBytes = 0;
            Memory<byte> bufferMemory = buffer.AsMemory();

            byte[] urlBuffer = UrlBuffer.Value;
            int[] urlLengths = UrlLengths.Value;
            int urlCount = 0;

            while (true)
            {
                int bytesRead = await responseStream.ReadAsync(
                    bufferMemory.Slice(remainingBytes, BufferSize),
                    cancellationToken).ConfigureAwait(false);

                if (bytesRead == 0)
                {
                    if (remainingBytes > 0)
                    {
                        // Process final buffer
                        ProcessBufferOptimized(
                            bufferMemory.Slice(0, remainingBytes).Span,
                            ref remainingBytes,
                            urlBuffer,
                            urlLengths,
                            ref urlCount,
                            isLastBuffer: true);

                        if (urlCount > 0)
                        {
                            StringCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, urlCount, MaxUrlLength, foundLinks);
                        }
                    }

                    break;
                }

                int totalBytes = remainingBytes + bytesRead;
                ProcessBufferOptimized(
                    bufferMemory.Slice(0, totalBytes).Span,
                    ref remainingBytes,
                    urlBuffer,
                    urlLengths,
                    ref urlCount,
                    isLastBuffer: false);

                if (remainingBytes > 0 && remainingBytes < totalBytes)
                {
                    bufferMemory.Slice(totalBytes - remainingBytes, remainingBytes)
                        .CopyTo(bufferMemory);
                }

                if (urlCount > 0)
                {
                    StringCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, urlCount, MaxUrlLength, foundLinks);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void ProcessBufferOptimized(
        Span<byte> buffer,
        ref int remainingBytes,
        byte[] urlBuffer,
        int[] urlLengths,
        ref int urlCount,
        bool isLastBuffer)
    {
        fixed (byte* bufPtr = buffer)
        fixed (byte* urlBufPtr = urlBuffer)
        {
            // Cache frequently accessed values
            int position = 0;
            int length = buffer.Length;
            int safeLength = length - 4;
            byte* endPtr = bufPtr + length;

            // Process in 4KB chunks for better cache utilization
            const int CHUNK_SIZE = 4096;
            byte* chunkEnd = bufPtr + Math.Min(CHUNK_SIZE, safeLength);

            while (position < safeLength)
            {
                // Prefetch next chunk
                if (position + CHUNK_SIZE < safeLength)
                {
                    Sse.Prefetch0(bufPtr + position + CHUNK_SIZE);
                }

                // Quick check for 'href' pattern
                int hrefPos = FindNextHrefOptimized(bufPtr + position, length - position);
                if (hrefPos == -1)
                {
                    remainingBytes = isLastBuffer ? 0 : Math.Min(MaxUrlLength, length - position);
                    return;
                }

                position += hrefPos;

                byte* currentPtr = bufPtr + position;
            
                // Find quotes and validate URL
                var quotePos = QuoteFinder.FindQuoteOptimized(currentPtr, (int)(endPtr - currentPtr));
                if (!quotePos.IsValid)
                {
                    if (!isLastBuffer && position + 4 >= safeLength)
                    {
                        remainingBytes = length - position;
                        return;
                    }
                    position += 4;
                    continue;
                }

                int urlStart = quotePos.Start;
                int urlLength = quotePos.Length;
            
                // Quick URL validation
                if (urlLength > 0 && urlLength < MaxUrlLength)
                {
                    byte* urlStartPtr = currentPtr + urlStart;
                
                    // Quick pre-check of first few chars
                    bool quickValid = true;
                    for (int i = 0; i < Math.Min(8, urlLength); i++)
                    {
                        byte c = urlStartPtr[i];
                        if (c >= 128 || ValidUrlChars[c] == 0)
                        {
                            quickValid = false;
                            break;
                        }
                    }

                    if (quickValid && IsValidUrl(urlStartPtr, urlLength))
                    {
                        int writePos = urlCount * MaxUrlLength;
                        int decodedLength = ProcessUrl(
                            urlStartPtr,
                            urlLength,
                            urlBufPtr + writePos);

                        if (decodedLength > 0)
                        {
                            urlLengths[urlCount++] = decodedLength;
                            if (urlCount >= urlLengths.Length)
                            {
                                return;
                            }
                        }
                    }
                }

                position += urlStart + urlLength;

                // Check if we've crossed chunk boundary
                if (position > (int)(chunkEnd - bufPtr))
                {
                    chunkEnd = bufPtr + Math.Min(position + CHUNK_SIZE, safeLength);
                }
            }

            remainingBytes = isLastBuffer ? 0 : Math.Min(MaxUrlLength, length - position);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int FindNextHrefOptimized(byte* buffer, int length)
    {
        if (length < 4) return -1;
        
        if (length >= 32)
        {
            int i = 0;
            while (i <= length - 32)
            {
                var data = Avx2.LoadVector256(buffer + i);
                var lower = Avx2.Or(data, LowerMask);
                var matches = Avx2.CompareEqual(lower, HChar);
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
}