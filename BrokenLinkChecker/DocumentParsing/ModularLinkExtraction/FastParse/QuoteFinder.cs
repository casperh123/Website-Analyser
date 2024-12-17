using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;

public class QuoteFinder
{
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe QuotePosition FindQuoteOptimized(byte* buffer, int length)
    {
        if (Avx2.IsSupported && length >= 32)
        {
            int i = 0;
            while (i <= length - 32)
            {
                // Quick scan for any quotes first
                byte* ptr = buffer + i;
                byte* end = ptr + 32;
                while (ptr < end)
                {
                    if (*ptr == '\'' || *ptr == '\"')
                    {
                        // Found quote, now find matching one
                        byte quote = *ptr;
                        int quotePos = (int)(ptr - buffer);
                        ptr++;

                        while (ptr < buffer + length)
                        {
                            if (*ptr == quote)
                            {
                                // Found matching quote - construct only once at the end
                                return new QuotePosition(
                                    quotePos + 1, 
                                    (int)(ptr - buffer) - (quotePos + 1), 
                                    true);
                            }
                            ptr++;
                        }

                        // Quote not matched, continue SIMD search
                        break;
                    }
                    ptr++;
                }

                // No quotes found in quick scan, use SIMD
                var data = Avx2.LoadVector256(buffer + i);
                var matches = Avx2.CompareEqual(data, QuoteChars);
                int mask = Avx2.MoveMask(matches);

                if (mask != 0)
                {
                    int quotePos = 0;
                    uint currentMask = (uint)mask;
                    while ((currentMask & 1) == 0)
                    {
                        currentMask >>= 1;
                        quotePos++;
                    }

                    byte quote = buffer[i + quotePos];
                    ptr = buffer + i + quotePos + 1;
                
                    while (ptr <= buffer + length - 32)
                    {
                        data = Avx2.LoadVector256(ptr);
                        matches = Avx2.CompareEqual(data, Vector256.Create(quote));
                        mask = Avx2.MoveMask(matches);

                        if (mask != 0)
                        {
                            int endPos = 0;
                            currentMask = (uint)mask;
                            while ((currentMask & 1) == 0)
                            {
                                currentMask >>= 1;
                                endPos++;
                            }

                            return new QuotePosition(
                                quotePos + 1, 
                                i + endPos - (quotePos + 1), 
                                true);
                        }
                    
                        ptr += 32;
                    }

                    // Check remaining bytes
                    while (ptr < buffer + length)
                    {
                        if (*ptr == quote)
                        {
                            return new QuotePosition(
                                quotePos + 1, 
                                (int)(ptr - buffer) - (quotePos + 1), 
                                true);
                        }
                        ptr++;
                    }
                }

                i += 32;
            }
        }

        return default;
    }
}