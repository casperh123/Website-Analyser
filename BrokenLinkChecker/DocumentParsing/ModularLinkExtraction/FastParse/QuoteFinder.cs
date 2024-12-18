using System.Numerics;
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
        if (length < 32 || !Avx2.IsSupported)
        {
            return FindQuoteScalar(buffer, 0, length);
        }

        int position = 0;
        int remainingLength = length;

        // Process full 32-byte blocks, but with overlap to handle cross-boundary cases
        while (position < length)
        {
            // Handle the final bytes if we're near the end
            if (length - position < 32)
            {
                return FindQuoteScalar(buffer, position, length - position);
            }

            // Load 32 bytes
            var data = Avx2.LoadVector256(buffer + position);
            var matches = Avx2.CompareEqual(data, QuoteChars);
            int mask = Avx2.MoveMask(matches);

            if (mask != 0)
            {
                int quoteOffset = BitOperations.TrailingZeroCount(mask);
                int quotePos = position + quoteOffset;
                
                var result = FindMatchingQuote(buffer, quotePos, length);
                if (result.IsValid)
                {
                    return result;
                }
            }

            // Move forward by 16 bytes instead of 32 to ensure we don't miss quotes that cross boundaries
            position += 16;
            remainingLength = length - position;
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe QuotePosition FindQuoteScalar(byte* buffer, int startPos, int length)
    {
        byte* ptr = buffer + startPos;
        byte* end = buffer + length; // Change to use total length instead of remaining length

        while (ptr < end)
        {
            if (*ptr == '\'' || *ptr == '\"')
            {
                int absolutePos = (int)(ptr - buffer);
                var result = FindMatchingQuote(buffer, absolutePos, length);
                if (result.IsValid)
                {
                    return result;
                }
            }
            ptr++;
        }

        return default;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe QuotePosition FindMatchingQuote(byte* buffer, int openQuotePos, int length)
    {
        byte quote = buffer[openQuotePos];
        byte* searchStart = buffer + openQuotePos + 1;
        byte* endPtr = buffer + length;

        for (byte* ptr = searchStart; ptr < endPtr; ptr++)
        {
            // Check for escaped quote
            if (*ptr == '\\' && ptr + 1 < endPtr && ptr[1] == quote)
            {
                ptr++;
                continue;
            }
            
            if (*ptr == quote)
            {
                int contentLength = (int)(ptr - (buffer + openQuotePos + 1));
                return new QuotePosition(openQuotePos, contentLength, true);
            }
        }

        return default;
    }
}