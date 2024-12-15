using System.Buffers;
using System.Numerics;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using System.Text;

public static class UltraFastLinkExtractor
{
    private const int BufferSize = 32768; // 32KB buffer
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;
    private static readonly Vector256<byte> HrefLowerMask = Vector256.Create((byte)'h');
    
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
                    buffer.AsMemory(remainingBytes, BufferSize - remainingBytes));
                    
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
    
    private static unsafe void ProcessBuffer(byte[] buffer, int bytesRead, ref int remainingBytes, List<string> foundLinks)
    {
        fixed (byte* bufPtr = buffer)
        {
            int position = 0;
            
            while (position < bytesRead - 4)
            {
                int hrefPos = FindNextHref(bufPtr + position, bytesRead - position);
                if (hrefPos == -1)
                {
                    remainingBytes = Math.Min(4, bytesRead - position);
                    return;
                }
                
                position += hrefPos;
                
                // Skip to opening quote
                while (position < bytesRead)
                {
                    byte c = bufPtr[position++];
                    if (c == '"' || c == '\'')
                    {
                        byte quote = c;
                        
                        // Find closing quote
                        int endPos = FindQuote(bufPtr + position, bytesRead - position, quote);
                        if (endPos == -1)
                        {
                            remainingBytes = bytesRead - position;
                            return;
                        }
                        
                        // Process link
                        if (endPos > 0)
                        {
                            var link = Encoding.ASCII.GetString(buffer, position, endPos).Trim();
                            if (link.Length > 0)
                            {
                                foundLinks.Add(link);
                            }
                        }
                        
                        position += endPos + 1;
                        break;
                    }
                    if (c == '>' || position >= bytesRead) break;
                }
            }
            
            remainingBytes = Math.Min(4, bytesRead - position);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int FindQuote(byte* buffer, int length, byte quote)
    {
        if (Avx2.IsSupported && length >= 32)
        {
            var vQuote = Vector256.Create(quote);
            
            int i = 0;
            while (i <= length - 32)
            {
                var data = Avx2.LoadVector256(buffer + i);
                var matches = Avx2.CompareEqual(data, vQuote);
                int mask = Avx2.MoveMask(matches);
                
                if (mask != 0)
                    return i + BitOperations.TrailingZeroCount(mask);
                
                i += 32;
            }
            
            length = i;
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
                var data = Avx2.LoadVector256(buffer + i);
                var normalized = Avx2.Or(data, caseMask);
                var matches = Avx2.CompareEqual(normalized, HrefLowerMask);
                int mask = Avx2.MoveMask(matches);
                
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
        
        for (int i = 0; i <= length - 4; i++)
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