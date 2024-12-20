using System.Text;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;


public unsafe class QuoteFinderHrefTests
{
    private static QuotePosition FindQuoteInString(string input)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(input);
        fixed (byte* ptr = bytes)
        {
            return QuoteFinder.FindQuoteOptimized(ptr, bytes.Length);
        }
    }

    [Theory]
    [InlineData("href=\"http://example.com\"", 5, 18)]      // Standard double quote
    [InlineData("href='http://example.com'", 5, 18)]        // Single quote
    [InlineData("href=\"/relative/path\"", 5, 14)]          // Relative path
    public void StandardHrefAttributes_FindsQuotedUrl(string html, int expectedStart, int expectedLength)
    {
        // Act
        var result = FindQuoteInString(html);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(expectedStart, result.Start);
        Assert.Equal(expectedLength, result.Length);
    }

    [Fact]
    public void LongUrl_SpanningBlocks_HandlesCorrectly()
    {
        // Arrange
        string longUrl = new string('x', 50);  // URL that will span 32-byte blocks
        string html = $"href=\"{longUrl}\"";

        // Act
        var result = FindQuoteInString(html);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(5, result.Start);
        Assert.Equal(50, result.Length);
    }

    [Theory]
    [InlineData("href=")]                    // No URL
    [InlineData("href=\"")]                  // Incomplete quote
    [InlineData("href=http://example.com")]  // No quotes
    public void InvalidHref_ReturnsNoMatch(string html)
    {
        // Act
        var result = FindQuoteInString(html);

        // Assert
        Assert.False(result.IsValid);
    }
    
    [Fact]
    public void UrlWithEscapedQuotes_ParsesCorrectly()
    {
        // Arrange
        string html = "href=\"https://example.com/page?q=\\\"quoted\\\"\"";

        // Act
        var result = FindQuoteInString(html);

        // Assert
        Assert.True(result.IsValid);
        Assert.Equal(5, result.Start);
        Assert.Equal(37, result.Length);
    }
}