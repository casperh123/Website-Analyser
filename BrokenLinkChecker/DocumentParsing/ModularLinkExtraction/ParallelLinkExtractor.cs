using System.Buffers;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using System.Text;
using System.Collections.Concurrent;
using System.Numerics;

public static class ParallelLinkExtractor
{
    // Optimize for L3 cache size (typically 4-12MB per socket)
    private const int ChunkSize = 64 * 1024; // 64KB chunks (typical L1 data cache size)
    private const int L2CacheSize = 512 * 1024; // 512KB (typical L2 cache size)
    private static readonly int L3CacheSize = GetL3CacheSize();
    private static readonly ArrayPool<byte> BufferPool = ArrayPool<byte>.Shared;
    private static readonly Vector256<byte> HrefLowerMask = Vector256.Create((byte)'h');

    private static int GetL3CacheSize()
    {
        // A reasonable default based on common CPU architectures
        return Environment.ProcessorCount * 2 * 1024 * 1024; // 2MB per core
    }

    public static async Task<List<string>> ExtractHrefsParallelAsync(Stream responseStream)
    {
        var links = new ConcurrentBag<string>();
        var streamLength = responseStream.Length;
        
        // Calculate optimal batch size based on cache sizes
        int batchSize = CalculateOptimalBatchSize(streamLength);
        var batches = CreateBatches(streamLength, batchSize);

        await Parallel.ForEachAsync(batches, new ParallelOptions 
        { 
            MaxDegreeOfParallelism = GetOptimalParallelism() 
        }, async (batch, ct) =>
        {
            await ProcessBatch(responseStream, batch, links);
        });

        return links.Distinct().ToList();
    }

    private static int GetOptimalParallelism()
    {
        // Use hardware threads minus 1 to leave room for IO
        return Math.Max(1, Environment.ProcessorCount - 1);
    }

    private static int CalculateOptimalBatchSize(long streamLength)
    {
        // Target L3 cache size while ensuring multiple batches per core
        int targetBatches = Environment.ProcessorCount * 4;
        long idealSize = streamLength / targetBatches;
        return (int)Math.Min(L3CacheSize / 2, Math.Max(ChunkSize, idealSize));
    }

    private static IEnumerable<BatchInfo> CreateBatches(long streamLength, int batchSize)
    {
        for (long position = 0; position < streamLength; position += batchSize)
        {
            yield return new BatchInfo
            {
                Start = position,
                Size = (int)Math.Min(batchSize, streamLength - position)
            };
        }
    }

    private static async Task ProcessBatch(Stream stream, BatchInfo batch, ConcurrentBag<string> links)
    {
        byte[] buffer = BufferPool.Rent(batch.Size);
        try
        {
            // Read batch
            lock (stream)
            {
                stream.Position = batch.Start;
                stream.ReadExactly(buffer, 0, batch.Size);
            }

            // Process in L2 cache-sized chunks
            for (int offset = 0; offset < batch.Size; offset += L2CacheSize)
            {
                int chunkSize = Math.Min(L2CacheSize, batch.Size - offset);
                ProcessChunk(buffer, offset, chunkSize, links);
            }
        }
        finally
        {
            BufferPool.Return(buffer);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void ProcessChunk(byte[] buffer, int offset, int size, ConcurrentBag<string> links)
    {
        fixed (byte* bufPtr = &buffer[offset])
        {
            int position = 0;

            while (position < size - 4)
            {
                // Process in L1 cache-sized blocks
                int blockSize = Math.Min(ChunkSize, size - position);
                position += ProcessBlock(bufPtr + position, blockSize, links);
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe int ProcessBlock(byte* buffer, int size, ConcurrentBag<string> links)
    {
        int position = 0;
        while (position < size - 4)
        {
            int hrefPos = FindNextHref(buffer + position, size - position);
            if (hrefPos == -1) return size;
            
            position += hrefPos;
            
            while (position < size)
            {
                byte c = buffer[position++];
                if (c == '"' || c == '\'')
                {
                    byte quote = c;
                    int endPos = FindQuote(buffer + position, size - position, quote);
                    if (endPos == -1) return position;
                    
                    if (endPos > 0)
                    {
                        ProcessLink(buffer + position, endPos, links);
                    }
                    
                    position += endPos + 1;
                    break;
                }
                if (c == '>' || position >= size) return position;
            }
        }
        return position;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void ProcessLink(byte* source, int length, ConcurrentBag<string> links)
    {
        // Skip whitespace using SIMD
        int start = 0;
        int end = length;

        if (length >= 32 && Avx2.IsSupported)
        {
            var spaces = Vector256.Create((byte)' ');
            var wsChars = Vector256.Create(
                (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r',
                (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r',
                (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r',
                (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r',
                (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r',
                (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r',
                (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r',
                (byte)' ', (byte)'\t', (byte)'\n', (byte)'\r'
            );

            // Trim start
            while (start <= length - 32)
            {
                var data = Avx2.LoadVector256(source + start);
                var matches = Avx2.CompareEqual(data, wsChars);
                var mask = Avx2.MoveMask(matches);
                
                if (mask != -1)
                {
                    start += BitOperations.TrailingZeroCount(~mask);
                    break;
                }
                start += 32;
            }

            // Trim end
            while (end >= start + 32)
            {
                var data = Avx2.LoadVector256(source + end - 32);
                var matches = Avx2.CompareEqual(data, wsChars);
                var mask = Avx2.MoveMask(matches);
                
                if (mask != -1)
                {
                    end -= BitOperations.LeadingZeroCount((uint)~mask);
                    break;
                }
                end -= 32;
            }
        }

        // Handle remaining bytes
        while (start < end && (source[start] <= 32)) start++;
        while (end > start && (source[end - 1] <= 32)) end--;

        if (end > start)
        {
            var link = Encoding.ASCII.GetString(source + start, end - start);
            links.Add(link);
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

    private class BatchInfo
    {
        public long Start { get; set; }
        public int Size { get; set; }
    }
}