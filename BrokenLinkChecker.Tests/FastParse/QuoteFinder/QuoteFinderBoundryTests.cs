using System.Text;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;
using Xunit;
using Xunit.Abstractions;

namespace BrokenLinkChecker.Tests.DocumentParsing.ModularLinkExtraction.FastParse;

public unsafe class QuoteFinderBoundaryTests
{
    private readonly ITestOutputHelper _output;

    public QuoteFinderBoundaryTests(ITestOutputHelper output)
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

    private static string GenerateTestString(int prefixLength, string quoteContent)
    {
        return new string('x', prefixLength) + quoteContent;
    }

    [Theory]
    [InlineData(30, "'test'", 31, 4)]  // Just under block size
    [InlineData(31, "'test'", 32, 4)]  // At block boundary
    [InlineData(32, "'test'", 33, 4)]  // Just over block size
    [InlineData(33, "'test'", 34, 4)]  // One past block size
    [InlineData(62, "'test'", 63, 4)]  // Just under two blocks
    [InlineData(63, "'test'", 64, 4)]  // At two blocks
    [InlineData(64, "'test'", 65, 4)]  // Just over two blocks
    public void NonAligned_QuotePosition_Verification(int prefixLength, string quote, 
        int expectedStart, int expectedLength)
    {
        // Arrange
        string input = GenerateTestString(prefixLength, quote);
        _output.WriteLine($"Input length: {input.Length}");
        _output.WriteLine($"Quote should start at: {expectedStart}");
        
        // Act
        var result = FindQuoteInString(input);
        
        // Assert
        _output.WriteLine($"IsValid: {result.IsValid}, Start: {result.Start}, Length: {result.Length}");
        Assert.True(result.IsValid);
        Assert.Equal(expectedStart, result.Start + 1);
        Assert.Equal(expectedLength, result.Length);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    public void SmallBuffer_WithQuotes_HandlesCorrectly(int paddingSize)
    {
        // Arrange
        string input = new string('x', paddingSize) + "'test'";
        
        // Act
        var result = FindQuoteInString(input);
        
        // Assert
        _output.WriteLine($"Buffer size: {input.Length}, IsValid: {result.IsValid}, Start: {result.Start}");
        Assert.True(result.IsValid);
        Assert.Equal(paddingSize + 1, result.Start + 1);
        Assert.Equal(4, result.Length);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(31)]
    [InlineData(32)]
    [InlineData(33)]
    [InlineData(34)]
    public void DetailedBoundaryAnalysis(int prefixLength)
    {
        // Arrange
        string input = new string('x', prefixLength) + "'y'";
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        
        // Log the byte pattern around the boundary
        _output.WriteLine($"String length: {input.Length}");
        _output.WriteLine($"Byte array length: {bytes.Length}");
        _output.WriteLine("Byte pattern around quote:");
        
        int start = Math.Max(0, prefixLength - 2);
        int end = Math.Min(bytes.Length, prefixLength + 4);
        for (int i = start; i < end; i++)
        {
            _output.WriteLine($"Position {i}: {(char)bytes[i]} ({bytes[i]})");
        }
        
        // Act
        var result = FindQuoteInString(input);
        
        // Assert
        _output.WriteLine($"Result - IsValid: {result.IsValid}, Start: {result.Start}, Length: {result.Length}");
        Assert.True(result.IsValid);
        Assert.Equal(prefixLength + 1, result.Start + 1);
        Assert.Equal(1, result.Length);
    }

    [Fact]
    public void CrossBoundary_QuoteContent()
    {
        // This test specifically checks quotes that span the 32-byte boundary
        string prefix = new string('x', 30);  // 30 bytes
        string quoteContent = "abcd";         // 4 bytes - will cross the boundary
        string input = $"{prefix}'{quoteContent}'";
        
        var result = FindQuoteInString(input);
        
        _output.WriteLine($"Input length: {input.Length}");
        _output.WriteLine($"Quote should start at: 31");
        _output.WriteLine($"IsValid: {result.IsValid}, Start: {result.Start}, Length: {result.Length}");
        
        Assert.True(result.IsValid);
        Assert.Equal(31, result.Start + 1);
        Assert.Equal(4, result.Length);
    }

    [Theory]
    [InlineData("'")]
    [InlineData("\"")]
    public void SingleCharacterQuotes_AtBoundary(string quote)
    {
        // Test both single and double quotes right at the boundary
        for (int i = 30; i <= 34; i++)
        {
            string input = new string('x', i) + quote + "y" + quote;
            var result = FindQuoteInString(input);
            
            _output.WriteLine($"Position {i}: IsValid={result.IsValid}, Start={result.Start}, Length={result.Length}");
            Assert.True(result.IsValid);
            Assert.Equal(i + 1, result.Start + 1);
            Assert.Equal(1, result.Length);
        }
    }
}