using System.Net.Http;
using System.Text;
using Xunit;
using RichardSzalay.MockHttp;
using System.IO;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;

public class StringCreatorHttpTests
{
    [Fact]
    public void EmptyBuffer_ReturnsEmptyList()
    {
        // Arrange
        byte[] buffer = Array.Empty<byte>();
        int[] lengths = Array.Empty<int>();
        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(buffer, lengths, 0, 100, output);

        // Assert
        Assert.Empty(output);
    }

    [Fact]
    public void SingleValidUrl_ReturnsCorrectString()
    {
        // Arrange
        const string expectedUrl = "https://example.com";
        byte[] urlBytes = Encoding.UTF8.GetBytes(expectedUrl);
        int maxLength = 100;
        
        byte[] buffer = new byte[maxLength];
        Array.Copy(urlBytes, buffer, urlBytes.Length);
        
        int[] lengths = new[] { urlBytes.Length };
        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(buffer, lengths, 1, maxLength, output);

        // Assert
        Assert.Single(output);
        Assert.Equal(expectedUrl, output[0]);
    }

    [Fact]
    public void MultipleUrls_WithDifferentLengths_ReturnsAllValidUrls()
    {
        // Arrange
        string[] expectedUrls = new[]
        {
            "https://short.com",
            "https://much-longer-domain-name-example.com/with/path",
            "https://another.com"
        };

        int maxLength = 100;
        byte[] buffer = new byte[maxLength * expectedUrls.Length];
        int[] lengths = new int[expectedUrls.Length];

        for (int i = 0; i < expectedUrls.Length; i++)
        {
            byte[] urlBytes = Encoding.UTF8.GetBytes(expectedUrls[i]);
            Array.Copy(urlBytes, 0, buffer, i * maxLength, urlBytes.Length);
            lengths[i] = urlBytes.Length;
        }

        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(buffer, lengths, expectedUrls.Length, maxLength, output);

        // Assert
        Assert.Equal(expectedUrls.Length, output.Count);
        Assert.Equal(expectedUrls, output);
    }

    [Fact]
    public void UrlsWithUnicode_ReturnsCorrectlyEncodedStrings()
    {
        // Arrange
        string[] expectedUrls = new[]
        {
            "https://example.com/🌟",
            "https://example.com/の",
            "https://example.com/ümlaut"
        };

        int maxLength = 100;
        byte[] buffer = new byte[maxLength * expectedUrls.Length];
        int[] lengths = new int[expectedUrls.Length];

        for (int i = 0; i < expectedUrls.Length; i++)
        {
            byte[] urlBytes = Encoding.UTF8.GetBytes(expectedUrls[i]);
            Array.Copy(urlBytes, 0, buffer, i * maxLength, urlBytes.Length);
            lengths[i] = urlBytes.Length;
        }

        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(buffer, lengths, expectedUrls.Length, maxLength, output);

        // Assert
        Assert.Equal(expectedUrls.Length, output.Count);
        Assert.Equal(expectedUrls, output);
    }

    [Fact]
    public void UrlsWithMaxLength_AreSkipped()
    {
        // Arrange
        string shortUrl = "https://short.com";
        string longUrl = "https://very-long-domain-name-that-exceeds-max-length.com/with/very/long/path/that/goes/on/and/on";
        
        int maxLength = 50;
        byte[] buffer = new byte[maxLength * 2];
        
        byte[] shortUrlBytes = Encoding.UTF8.GetBytes(shortUrl);
        byte[] longUrlBytes = Encoding.UTF8.GetBytes(longUrl);
        
        Array.Copy(shortUrlBytes, 0, buffer, 0, shortUrlBytes.Length);
        Array.Copy(longUrlBytes, 0, buffer, maxLength, Math.Min(longUrlBytes.Length, maxLength));
        
        int[] lengths = new[] { shortUrlBytes.Length, longUrlBytes.Length };
        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(buffer, lengths, 2, maxLength, output);

        // Assert
        Assert.Single(output);
        Assert.Equal(shortUrl, output[0]);
    }

    [Fact]
    public void UrlsWithZeroLength_AreSkipped()
    {
        // Arrange
        string validUrl = "https://example.com";
        int maxLength = 100;
        
        byte[] buffer = new byte[maxLength * 3];
        byte[] validUrlBytes = Encoding.UTF8.GetBytes(validUrl);
        
        Array.Copy(validUrlBytes, 0, buffer, maxLength, validUrlBytes.Length);
        
        int[] lengths = new[] { 0, validUrlBytes.Length, 0 };
        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(buffer, lengths, 3, maxLength, output);

        // Assert
        Assert.Single(output);
        Assert.Equal(validUrl, output[0]);
    }

    [Fact]
    public void LargeNumberOfUrls_HandlesEfficiently()
    {
        // Arrange
        const int urlCount = 10000;
        const string baseUrl = "https://example.com/";
        int maxLength = 100;
        
        byte[] buffer = new byte[maxLength * urlCount];
        int[] lengths = new int[urlCount];
        var expectedUrls = new List<string>();

        for (int i = 0; i < urlCount; i++)
        {
            string url = $"{baseUrl}{i}";
            expectedUrls.Add(url);
            byte[] urlBytes = Encoding.UTF8.GetBytes(url);
            Array.Copy(urlBytes, 0, buffer, i * maxLength, urlBytes.Length);
            lengths[i] = urlBytes.Length;
        }

        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(buffer, lengths, urlCount, maxLength, output);

        // Assert
        Assert.Equal(urlCount, output.Count);
        Assert.Equal(expectedUrls, output);
    }

    [Theory]
    [InlineData(new byte[] { 0xF0, 0x9F, 0x98, 0x81 })] // Valid UTF-8 emoji (😁)
    [InlineData(new byte[] { 0xE2, 0x82, 0xAC })] // Valid UTF-8 euro symbol (€)
    [InlineData(new byte[] { 0xC2, 0xA9 })] // Valid UTF-8 copyright symbol (©)
    public void ValidUtf8Sequences_AreHandledCorrectly(byte[] validSequence)
    {
        // Arrange
        int maxLength = 100;
        byte[] buffer = new byte[maxLength];
        Array.Copy(validSequence, buffer, validSequence.Length);
        int[] lengths = new[] { validSequence.Length };
        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(buffer, lengths, 1, maxLength, output);

        // Assert
        Assert.Single(output);
        Assert.Equal(Encoding.UTF8.GetString(validSequence), output[0]);
    }
}