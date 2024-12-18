using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;

public class QuoteFinder
{
    // Simplified vector creation - only need single/double quote alternating pattern
    private static readonly Vector256<byte> QuoteChars = Vector256.Create(
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"',
        (byte)'\'', (byte)'\"'
    );

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe QuotePosition FindQuoteOptimized(byte* buffer, int length)
    {
        if (length < 32 || !Avx2.IsSupported)
        {
            return FindQuoteScalar(buffer, length);
        }

        // Process chunks with overlap
        for (int pos = 0; pos <= length - 32; pos += 24)
        {
            int mask = Avx2.MoveMask(
                Avx2.CompareEqual(
                    Avx2.LoadVector256(buffer + pos),
                    QuoteChars
                )
            );

            while (mask != 0)
            {
                int quotePos = pos + BitOperations.TrailingZeroCount(mask);
                var result = TryMatchQuote(buffer, quotePos, length);
                
                if (result.IsValid)
                {
                    return result;
                }
                
                mask &= mask - 1; // Clear lowest set bit
            }
        }

        // Handle remaining bytes
        return FindQuoteScalar(buffer + (length - 32), 32);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe QuotePosition FindQuoteScalar(byte* buffer, int length)
    {
        byte* end = buffer + length;
        
        for (byte* ptr = buffer; ptr < end; ptr++)
        {
            if (*ptr == '\'' || *ptr == '\"')
            {
                var result = TryMatchQuote(buffer, (int)(ptr - buffer), length);
                if (result.IsValid)
                {
                    return result;
                }
            }
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe QuotePosition TryMatchQuote(byte* buffer, int openQuotePos, int length)
    {
        byte quote = buffer[openQuotePos];
        byte* ptr = buffer + openQuotePos + 1;
        byte* end = buffer + length;

        while (ptr < end)
        {
            if (*ptr == '\\' && ptr + 1 < end && ptr[1] == quote)
            {
                ptr += 2;
                continue;
            }
            
            if (*ptr == quote)
            {
                return new QuotePosition(
                    openQuotePos,
                    (int)(ptr - (buffer + openQuotePos + 1)),
                    true
                );
            }
            
            ptr++;
        }

        return default;
    }
}