using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using System.Text;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;
using Xunit;
using Xunit.Abstractions;

namespace BrokenLinkChecker.Tests.DocumentParsing.ModularLinkExtraction.FastParse;

public unsafe class QuoteFinderPerformanceValidationTests
{
    private readonly ITestOutputHelper _output;
    private const int MaxBitPackedValue = 0x1FFF; // 8191 - maximum value for bit-packed fields

    public QuoteFinderPerformanceValidationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    private static QuotePosition FindQuoteInString(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        fixed (byte* ptr = bytes)
        {
            return QuoteFinder.FindQuoteOptimized(ptr, bytes.Length);
        }
    }

    private string GenerateTestString(int length, int quotePosition, char quoteChar = '\'')
    {
        if (quotePosition >= length - 1)
            throw new ArgumentException("Quote position must allow for closing quote");

        StringBuilder sb = new StringBuilder(length);
        sb.Append('x', quotePosition);
        sb.Append(quoteChar);
        sb.Append('y', length - quotePosition - 2);
        sb.Append(quoteChar);
        return sb.ToString();
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(10000)]
    [InlineData(100000)]
    public void LongString_QuotesAtStart_Performance(int length)
    {
        // Arrange
        string input = GenerateTestString(length, 0);
        
        // Act
        var sw = Stopwatch.StartNew();
        var result = FindQuoteInString(input);
        sw.Stop();

        // Assert
        _output.WriteLine($"Length {length}: {sw.ElapsedMilliseconds}ms");
        Assert.True(result.IsValid);
        Assert.Equal(1, result.Start + 1);
        Assert.Equal(Math.Min(length - 2, MaxBitPackedValue), result.Length);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(10000)]
    [InlineData(100000)]
    public void LongString_QuotesAtEnd_Performance(int length)
    {
        // Arrange
        string input = GenerateTestString(length, length - 6);  // "xxxxx'y'"
        
        // Act
        var sw = Stopwatch.StartNew();
        var result = FindQuoteInString(input);
        sw.Stop();

        // Assert
        _output.WriteLine($"Length {length}: {sw.ElapsedMilliseconds}ms");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void MixedQuoteTypes_LargeBuffer_Performance()
    {
        // Arrange
        const int bufferSize = 100000;
        StringBuilder sb = new StringBuilder(bufferSize);
        Random rng = new Random(42); // Fixed seed for reproducibility
        
        int expectedStart = -1;
        char[] quoteChars = { '\'', '"' };
        
        for (int i = 0; i < bufferSize; i++)
        {
            if (rng.Next(100) < 2) // 2% chance of quote
            {
                char quote = quoteChars[rng.Next(2)];
                if (expectedStart == -1)
                    expectedStart = i + 1;
                sb.Append(quote);
            }
            else
            {
                sb.Append('x');
            }
        }

        string input = sb.ToString();
        
        // Act
        var sw = Stopwatch.StartNew();
        var result = FindQuoteInString(input);
        sw.Stop();

        // Assert
        _output.WriteLine($"Mixed quotes processing time: {sw.ElapsedMilliseconds}ms");
        Assert.True(result.IsValid);
        Assert.Equal(expectedStart, result.Start + 1);
    }

    [Theory]
    [InlineData(32)]    // Exactly one SIMD block
    [InlineData(64)]    // Two SIMD blocks
    [InlineData(96)]    // Three SIMD blocks
    [InlineData(128)]   // Four SIMD blocks
    [InlineData(31)]    // Just under SIMD block
    [InlineData(33)]    // Just over SIMD block
    [InlineData(256)]
    [InlineData(512)] 
    public void DifferentBufferSizes_Performance(int size)
    {
        // Arrange
        string input = GenerateTestString(size, size / 2);  // Quote in middle
        
        // Act
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)  // Run multiple times for better timing
        {
            var result = FindQuoteInString(input);
            Assert.True(result.IsValid);
        }
        sw.Stop();

        // Assert
        _output.WriteLine($"Buffer size {size}: {sw.ElapsedMilliseconds}ms for 1000 iterations");
    }

    [Fact]
    public void PathologicalCase_AlternatingQuotes()
    {
        // Arrange
        const int size = 10000;
        StringBuilder sb = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            sb.Append(i % 2 == 0 ? '\'' : '"');
        }
        
        // Act
        var sw = Stopwatch.StartNew();
        var result = FindQuoteInString(sb.ToString());
        sw.Stop();

        // Assert
        _output.WriteLine($"Pathological case processing time: {sw.ElapsedMilliseconds}ms");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void PathologicalCase_QuotesAtSIMDBoundaries()
    {
        // Arrange
        const int numBlocks = 100;
        StringBuilder sb = new StringBuilder(32 * numBlocks);
        
        for (int i = 0; i < numBlocks; i++)
        {
            sb.Append(new string('x', 31));
            sb.Append(i % 2 == 0 ? '\'' : '"');
        }
        
        // Act
        var sw = Stopwatch.StartNew();
        var result = FindQuoteInString(sb.ToString());
        sw.Stop();

        // Assert
        _output.WriteLine($"SIMD boundary pathological case: {sw.ElapsedMilliseconds}ms");
        Assert.True(result.IsValid);
    }

    [Fact]
    public void StressTest_RandomQuotePlacements()
    {
        // Arrange
        const int iterations = 1000;
        const int maxSize = 10000;
        Random rng = new Random(42);
        var sw = new Stopwatch();
        
        // Act
        for (int i = 0; i < iterations; i++)
        {
            int size = rng.Next(32, maxSize);
            int quotePos = rng.Next(0, size - 2);
            char quoteType = rng.Next(2) == 0 ? '\'' : '"';
            
            string input = GenerateTestString(size, quotePos, quoteType);
            
            sw.Start();
            var result = FindQuoteInString(input);
            sw.Stop();
            
            Assert.True(result.IsValid);
            Assert.Equal(quotePos + 1, result.Start + 1);
        }

        // Assert
        _output.WriteLine($"Stress test average time: {sw.ElapsedMilliseconds / (double)iterations}ms per iteration");
    }

    [Fact]
    public void LargeFile_Simulation()
    {
        // Simulate processing a large HTML file with scattered quotes
        const int fileSize = 1_000_000;
        const int avgTagSize = 50;
        StringBuilder sb = new StringBuilder(fileSize);
        Random rng = new Random(42);

        // Build a pseudo-HTML structure
        int position = 0;
        while (position < fileSize)
        {
            // Add some text
            int textLength = rng.Next(10, 100);
            sb.Append('x', Math.Min(textLength, fileSize - position));
            position += textLength;

            if (position >= fileSize) break;

            // Add a tag with attributes
            sb.Append('<');
            sb.Append('x', 5); // tag name
            sb.Append(' ');
            
            // Add some attributes with quotes
            int numAttributes = rng.Next(1, 5);
            for (int i = 0; i < numAttributes && position < fileSize; i++)
            {
                char quoteType = rng.Next(2) == 0 ? '\'' : '"';
                sb.Append($"attr{i}={quoteType}value{i}{quoteType} ");
            }
            
            sb.Append('>');
        }

        string input = sb.ToString();

        // Act
        var sw = Stopwatch.StartNew();
        var result = FindQuoteInString(input);
        sw.Stop();

        // Assert
        _output.WriteLine($"Large file processing time: {sw.ElapsedMilliseconds}ms");
        Assert.True(result.IsValid);
    }
}