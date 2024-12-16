using System.Buffers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Web;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;

public static class UltraFastLinkExtractor
{
    private const int BufferSize = 1028 * 64; // 32KB buffer
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;
    private static readonly Vector256<byte> HrefLowerMask = Vector256.Create((byte)'h');
    
    private static readonly ThreadLocal<byte[]> UrlBuffer = new(() => new byte[8192]);
    private static readonly ThreadLocal<char[]> CharBuffer = new(() => new char[4096]);
    
    public static async Task<List<string>> ExtractHrefsAsync(Stream responseStream)
    {
        var foundLinks = new List<string>();
        byte[] buffer = BufferPool.Rent(BufferSize);
        int remainingBytes = 0;
        
        try
        {
            while (true)
            {
                int bytesRead = await responseStream.ReadAsync(
                    buffer.AsMemory(remainingBytes, BufferSize - remainingBytes)).ConfigureAwait(false);
                    
                if (bytesRead == 0) break;
                bytesRead += remainingBytes;
                
                ProcessBuffer(buffer, bytesRead, ref remainingBytes, foundLinks);
                
                if (remainingBytes > 0 && remainingBytes < bytesRead)
                {
                    Buffer.BlockCopy(buffer, bytesRead - remainingBytes, buffer, 0, remainingBytes);
                }
            }
            
            return foundLinks;
        }
        finally
        {
            BufferPool.Return(buffer);
        }
    }

    private static unsafe void ProcessBuffer(byte[] buffer, int bytesRead, ref int remainingBytes,
        List<string> foundLinks)
    {
        // These could be constants to avoid stack allocation
        const byte colon = (byte)'\'';
        const byte gooseeye = (byte)'\"';

        fixed (byte* bufPtr = buffer)
        {
            int position = 0;
            // Avoid checking bytesRead - 4 on every iteration
            int safeLength = bytesRead - 4;

            while (position < safeLength)
            {
                int hrefPos = FindNextHref(bufPtr + position, bytesRead - position);
                if (hrefPos == -1)
                {
                    remainingBytes = Math.Min(4, bytesRead - position);
                    return;
                }

                position += hrefPos;

                // We can use pointer arithmetic more efficiently here
                byte* currentPtr = bufPtr + position;
                byte* endPtr = bufPtr + bytesRead;

                // Find quote character
                while (currentPtr < endPtr)
                {
                    byte c = *currentPtr++;

                    if (c == gooseeye || c == colon)
                    {
                        // Find closing quote using SIMD
                        int endPos = FindQuote(currentPtr, (int)(endPtr - currentPtr), c);
                        if (endPos == -1)
                        {
                            remainingBytes = (int)(endPtr - currentPtr);
                            return;
                        }

                        // Zero-copy string creation for valid URLs
                        if (endPos > 0 && endPos < 2048) // Reasonable URL length limit
                        {
                            byte* urlStart = currentPtr;
                            int urlLength = endPos;
    
                            // Quick validation - check for common valid URL chars
                            bool isValid = true;
                            for (int i = 0; i < urlLength; i++)
                            {
                                byte ca = urlStart[i];
                                if (ca < 32 || ca > 126)
                                {
                                    isValid = false;
                                    break;
                                }
                            }
    
                            if (isValid)
                            {
                                // Use thread-local buffers for zero-allocation processing
                                var urlBytes = UrlBuffer.Value;
                                var charBuffer = CharBuffer.Value;
        
                                // Copy and basic URL decode in one pass
                                int decodedLength = 0;
                                for (int i = 0; i < urlLength && decodedLength < charBuffer.Length - 1; i++)
                                {
                                    byte cas = urlStart[i];
            
                                    if (cas == '%' && i + 2 < urlLength)
                                    {
                                        // Fast hex decode
                                        int high = HexValue(urlStart[i + 1]);
                                        int low = HexValue(urlStart[i + 2]);
                
                                        if (high >= 0 && low >= 0)
                                        {
                                            charBuffer[decodedLength++] = (char)((high << 4) | low);
                                            i += 2;
                                            continue;
                                        }
                                    }
            
                                    charBuffer[decodedLength++] = (char)cas;
                                }
        
                                // Create final string only once
                                if (decodedLength > 0)
                                {
                                    foundLinks.Add(new string(charBuffer, 0, decodedLength));
                                }
                            }
    
                            position = (int)(currentPtr - bufPtr) + endPos + 1;
                            break;
                        }

                        if (c == (byte)'>' || currentPtr >= endPtr) break;
                    }
                }

                remainingBytes = Math.Min(4, bytesRead - position);
            }
        }
    }
    
    // Helper method for hex decoding
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int HexValue(byte b)
    {
        if (b >= '0' && b <= '9') return b - '0';
        if (b >= 'a' && b <= 'f') return b - 'a' + 10;
        if (b >= 'A' && b <= 'F') return b - 'A' + 10;
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int FindQuote(byte* buffer, int length, byte quote)
    {
        if (Avx2.IsSupported && length >= 64) // Increased to process more data at once
        {
            Vector256<byte> vQuote = Vector256.Create(quote);
        
            // Process two vectors at once to better utilize CPU pipeline
            int i = 0;
            while (i <= length - 64)
            {
                var data1 = Avx2.LoadVector256(buffer + i);
                var data2 = Avx2.LoadVector256(buffer + i + 32);
                var matches1 = Avx2.CompareEqual(data1, vQuote);
                var matches2 = Avx2.CompareEqual(data2, vQuote);
                int mask1 = Avx2.MoveMask(matches1);
                int mask2 = Avx2.MoveMask(matches2);
            
                if (mask1 != 0)
                    return i + BitOperations.TrailingZeroCount(mask1);
                if (mask2 != 0)
                    return i + 32 + BitOperations.TrailingZeroCount(mask2);
            
                i += 64;
            }
        }
        
        for (int i = 0; i < length; i++)
            if (buffer[i] == quote) return i;
        
        return -1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int FindNextHref(byte* buffer, int length)
    {
        if (Avx2.IsSupported && length >= 32)
        {
            var caseMask = Vector256.Create((byte)0x20);
            
            int i = 0;
            while (i <= length - 32)
            {
                Vector256<byte> data = Avx2.LoadVector256(buffer + i);
                Vector256<byte> normalized = Avx2.Or(data, caseMask);
                Vector256<byte> matches = Avx2.CompareEqual(normalized, HrefLowerMask);
                var mask = Avx2.MoveMask(matches);
                
                while (mask != 0)
                {
                    int pos = i + BitOperations.TrailingZeroCount(mask);
                    if (pos <= length - 4)
                    {
                        uint word = *(uint*)(buffer + pos) | 0x20202020;
                        if ((word & 0xFF) == 'h' &&
                            ((word >> 8) & 0xFF) == 'r' &&
                            ((word >> 16) & 0xFF) == 'e' &&
                            ((word >> 24) & 0xFF) == 'f')
                        {
                            return pos;
                        }
                    }
                    mask &= mask - 1;
                }
                i += 32 - 3;
            }
            
            length = i;
        }
        
        for (int i = 0; i <= length - 16; i += 4)
        {
            uint word1 = *(uint*)(buffer + i) | 0x20202020;
            uint word2 = *(uint*)(buffer + i + 1) | 0x20202020;
            uint word3 = *(uint*)(buffer + i + 2) | 0x20202020;
            uint word4 = *(uint*)(buffer + i + 3) | 0x20202020;

            if ((word1 & 0xFF) == 'h' &&
                ((word1 >> 8) & 0xFF) == 'r' &&
                ((word1 >> 16) & 0xFF) == 'e' &&
                ((word1 >> 24) & 0xFF) == 'f')
            {
                return i;
            }
            if ((word2 & 0xFF) == 'h' &&
                ((word2 >> 8) & 0xFF) == 'r' &&
                ((word2 >> 16) & 0xFF) == 'e' &&
                ((word2 >> 24) & 0xFF) == 'f')
            {
                return i + 1;
            }
            if ((word3 & 0xFF) == 'h' &&
                ((word3 >> 8) & 0xFF) == 'r' &&
                ((word3 >> 16) & 0xFF) == 'e' &&
                ((word3 >> 24) & 0xFF) == 'f')
            {
                return i + 2;
            }
            if ((word4 & 0xFF) == 'h' &&
                ((word4 >> 8) & 0xFF) == 'r' &&
                ((word4 >> 16) & 0xFF) == 'e' &&
                ((word4 >> 24) & 0xFF) == 'f')
            {
                return i + 3;
            }
        }

        for (int i = length - 4; i <= length - 4; i++)
        {
            uint word = *(uint*)(buffer + i) | 0x20202020;
            if ((word & 0xFF) == 'h' &&
                ((word >> 8) & 0xFF) == 'r' &&
                ((word >> 16) & 0xFF) == 'e' &&
                ((word >> 24) & 0xFF) == 'f')
            {
                return i;
            }
        }
        
        return -1;
    }
}