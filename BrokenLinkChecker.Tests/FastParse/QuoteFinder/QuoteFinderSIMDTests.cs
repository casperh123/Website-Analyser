using System.Text;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;
using Xunit;
using Xunit.Abstractions;

namespace BrokenLinkChecker.Tests.DocumentParsing.ModularLinkExtraction.FastParse;

public unsafe class QuoteFinderSIMDTests
{
    private readonly ITestOutputHelper _output;

    public QuoteFinderSIMDTests(ITestOutputHelper output)
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

    [Theory]
    [InlineData(31)]   // Just before SIMD block
    [InlineData(32)]   // At SIMD block boundary
    [InlineData(33)]   // Just after SIMD block
    [InlineData(63)]   // Just before second SIMD block
    [InlineData(64)]   // At second SIMD block boundary
    [InlineData(65)]   // Just after second SIMD block
    public void QuotesAtSIMDBoundaries_PreservesPosition(int prefixLength)
    {
        // Arrange
        string prefix = new string('x', prefixLength);
        string input = $"{prefix}'test'";
        
        // Log setup for debugging
        _output.WriteLine($"Input length: {input.Length}");
        _output.WriteLine($"Expected quote position: {prefixLength}");

        // Act
        var result = FindQuoteInString(input);

        // Assert
        _output.WriteLine($"Result: Start={result.Start}, Length={result.Length}, IsValid={result.IsValid}");
        Assert.True(result.IsValid);
        Assert.Equal(prefixLength + 1, result.Start + 1);
        Assert.Equal(4, result.Length);
    }

    [Theory]
    [InlineData(28, 4)]  // Quote content spans first block boundary
    [InlineData(60, 4)]  // Quote content spans second block boundary
    [InlineData(92, 4)]  // Quote content spans third block boundary
    public void QuoteContent_SpanningSIMDBoundaries_HandlesCorrectly(int prefixLength, int contentLength)
    {
        // Arrange
        string content = new string('y', contentLength);
        string input = new string('x', prefixLength) + $"'{content}'";
        
        // Log setup for debugging
        _output.WriteLine($"Input length: {input.Length}");
        _output.WriteLine($"Quote start: {prefixLength}, Content length: {contentLength}");
        _output.WriteLine($"Quote spans boundary: {(prefixLength + contentLength) / 32} to {(prefixLength + contentLength + 1) / 32}");

        // Act
        var result = FindQuoteInString(input);

        // Assert
        _output.WriteLine($"Result: Start={result.Start}, Length={result.Length}, IsValid={result.IsValid}");
        Assert.True(result.IsValid);
        Assert.Equal(prefixLength + 1, result.Start + 1);
        Assert.Equal(contentLength, result.Length);
    }

    [Fact]
    public void LargeQuoteContent_AcrossMultipleSIMDBlocks_HandlesCorrectly()
    {
        // Arrange
        const int prefixLength = 30;
        const int contentLength = 100;  // Will span multiple SIMD blocks
        string content = new string('y', contentLength);
        string input = new string('x', prefixLength) + $"'{content}'";

        // Act
        var result = FindQuoteInString(input);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(prefixLength + 1, result.Start + 1);
        Assert.Equal(contentLength, result.Length);
    }

    [Theory]
    [InlineData(31, 32, 1)]  // Quotes straddle first block boundary
    [InlineData(63, 64, 1)]  // Quotes straddle second block boundary
    [InlineData(95, 96, 1)]  // Quotes straddle third block boundary
    public void MatchingQuotes_StraddlingSIMDBoundaries_HandlesCorrectly(
        int openQuotePos, int closeQuotePos, int expectedLength)
    {
        // Arrange
        string input = new string('x', openQuotePos) + "'" + 
                      new string('y', closeQuotePos - openQuotePos - 1) + "'";
        
        // Log setup for debugging
        _output.WriteLine($"Input length: {input.Length}");
        _output.WriteLine($"Open quote at: {openQuotePos}, Close quote at: {closeQuotePos}");

        // Act
        var result = FindQuoteInString(input);

        // Assert
        _output.WriteLine($"Result: Start={result.Start}, Length={result.Length}, IsValid={result.IsValid}");
        Assert.True(result.IsValid);
        Assert.Equal(openQuotePos + 1, result.Start + 1);
        Assert.Equal(expectedLength, result.Length + 1);
    }

    [Theory]
    [InlineData("'", 31)]   // Single quote at first block boundary
    [InlineData("\"", 31)]  // Double quote at first block boundary
    [InlineData("'", 63)]   // Single quote at second block boundary
    [InlineData("\"", 63)]  // Double quote at second block boundary
    public void DifferentQuoteTypes_AtSIMDBoundaries_HandlesCorrectly(string quoteChar, int position)
    {
        // Arrange
        string input = new string('x', position) + $"{quoteChar}y{quoteChar}";
        
        // Act
        var result = FindQuoteInString(input);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(position + 1, result.Start + 1);
        Assert.Equal(1, result.Length);
    }

    [Fact]
    public void MultipleQuotes_AcrossSIMDBoundaries_FindsFirst()
    {
        // Arrange - Create a string with quotes spanning SIMD blocks
        var prefix = new string('x', 31);      // 31 bytes before first quote
        var firstQuote = "'first'";            // 7 bytes (including quotes)
        var middle = new string('x', 32);      // 32 bytes between quotes
        var secondQuote = "'second'";          // 8 bytes (including quotes)
        var input = prefix + firstQuote + middle + secondQuote;

        // Log the structure for debugging
        _output.WriteLine($"String structure:");
        _output.WriteLine($"Prefix length: {prefix.Length}");
        _output.WriteLine($"First quote starts at: {prefix.Length}");
        _output.WriteLine($"First quote content length: {firstQuote.Length - 2}");
        _output.WriteLine($"Total input length: {input.Length}");

        // Act
        var result = FindQuoteInString(input);

        // Log the result
        _output.WriteLine($"Result: Start={result.Start}, Length={result.Length}, IsValid={result.IsValid}");
        
        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(31 + 1, result.Start + 1);  // Should find quote after 31 characters
        Assert.Equal(5, result.Length);          // 'first' is 5 characters
    }
}