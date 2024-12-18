using System.Text;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;
using Xunit;

namespace BrokenLinkChecker.Tests.DocumentParsing.ModularLinkExtraction.FastParse;

public unsafe class QuoteFinderBasicValidityTests
{
    private static QuotePosition FindQuoteInString(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        fixed (byte* ptr = bytes)
        {
            return QuoteFinder.FindQuoteOptimized(ptr, bytes.Length);
        }
    }

    [Fact]
    public void EmptyString_ReturnsDefaultPosition()
    {
        // Arrange
        string input = "";

        // Act
        var result = FindQuoteInString(input);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(0, result.Start);
        Assert.Equal(0, result.Length);
    }

    [Theory]
    [InlineData("'test'", 1, 4)]            // Single quotes
    [InlineData("\"test\"", 1, 4)]          // Double quotes
    [InlineData("abc'test'def", 4, 4)]      // Quotes with surrounding content
    [InlineData("'test\"", 0, 0)]           // Mismatched quotes
    public void BasicQuoteMatching_ReturnsCorrectPosition(string input, int expectedStart, int expectedLength)
    {
        // Act
        var result = FindQuoteInString(input);

        // Assert
        if (expectedLength > 0)
        {
            Assert.True(result.IsValid);
            Assert.Equal(expectedStart, result.Start + 1);
            Assert.Equal(expectedLength, result.Length);
        }
        else
        {
            Assert.False(result.IsValid);
        }
    }

    [Theory]
    [InlineData("no quotes here")]
    [InlineData("just some text")]
    [InlineData("1234567890")]
    public void NoQuotes_ReturnsDefaultPosition(string input)
    {
        // Act
        var result = FindQuoteInString(input);

        // Assert
        Assert.False(result.IsValid);
        Assert.Equal(0, result.Start);
        Assert.Equal(0, result.Length);
    }

    [Theory]
    [InlineData("'", false)]                 // Single quote alone
    [InlineData("\"", false)]                // Double quote alone
    [InlineData("text'", false)]             // Quote at end
    [InlineData("'text", false)]             // Quote at start without matching
    public void UnpairedQuotes_HandleCorrectly(string input, bool shouldFind)
    {
        // Act
        var result = FindQuoteInString(input);

        // Assert
        Assert.Equal(shouldFind, result.IsValid);
        if (!shouldFind)
        {
            Assert.Equal(0, result.Start);
            Assert.Equal(0, result.Length);
        }
    }
    

    [Theory]
    [InlineData("'te\\'st'")]               // Escaped single quote
    [InlineData("\"te\\\"st\"")]            // Escaped double quote
    public void EscapedQuotes_HandleCorrectly(string input)
    {
        // Act
        var result = FindQuoteInString(input);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void LongContent_BetweenQuotes_HandlesCorrectly()
    {
        // Arrange
        string content = new string('x', 100);  // 100 'x' characters
        string input = $"'{content}'";

        // Act
        var result = FindQuoteInString(input);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(1, result.Start + 1);
        Assert.Equal(100, result.Length);
    }

    [Theory]
    [InlineData("'test'\"another\"")]       // Multiple quote pairs
    public void MultipleQuotePairs_ReturnsFirst(string input)
    {
        // Act
        var result = FindQuoteInString(input);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(1, result.Start + 1);
        Assert.Equal(4, result.Length);
    }
    
}