using System;
using System.Collections.Generic;
using System.Text;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;
using Xunit;

public class UrlCreaterTests
{
    [Fact]
    public void ConvertUrlsToStrings_EmptyInput_ReturnsEmptyList()
    {
        // Arrange
        byte[] urlBuffer = Array.Empty<byte>();
        int[] urlLengths = Array.Empty<int>();
        var output = new List<string>();

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, 0, 100, output);

        // Assert
        Assert.Empty(output);
    }

    [Fact]
    public void ConvertUrlsToStrings_SingleSimpleUrl_ConvertsCorrectly()
    {
        // Arrange
        const string testUrl = "https://example.com";
        var urlBuffer = new byte[100];
        var urlLengths = new int[] { testUrl.Length };
        var output = new List<string>();

        // Convert string to bytes
        Encoding.ASCII.GetBytes(testUrl, 0, testUrl.Length, urlBuffer, 0);

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, 1, 100, output);

        // Assert
        Assert.Single(output);
        Assert.Equal(testUrl, output[0]);
    }

    [Theory]
    [InlineData(15)]    // Tests SSE2 path
    [InlineData(31)]    // Tests AVX2 path
    [InlineData(33)]    // Tests AVX2 path with remainder
    [InlineData(63)]    // Tests multiple AVX2 iterations
    [InlineData(7)]     // Tests scalar path
    public void ConvertUrlsToStrings_VariousLengths_ConvertsCorrectly(int urlLength)
    {
        // Arrange
        var testUrl = new string('a', urlLength);
        var urlBuffer = new byte[100];
        var urlLengths = new int[] { urlLength };
        var output = new List<string>();

        Encoding.ASCII.GetBytes(testUrl, 0, testUrl.Length, urlBuffer, 0);

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, 1, 100, output);

        // Assert
        Assert.Single(output);
        Assert.Equal(testUrl, output[0]);
        Assert.Equal(urlLength, output[0].Length);
    }

    [Fact]
    public void ConvertUrlsToStrings_MultipleUrls_ConvertsAllCorrectly()
    {
        // Arrange
        string[] testUrls = new[]
        {
            "https://example.com",
            "https://test.org",
            "https://very-long-domain-name-for-testing.com/path/to/resource"
        };

        int maxLength = 100;
        var urlBuffer = new byte[maxLength * testUrls.Length];
        var urlLengths = new int[testUrls.Length];
        var output = new List<string>();

        // Setup test data
        for (int i = 0; i < testUrls.Length; i++)
        {
            Encoding.ASCII.GetBytes(testUrls[i], 0, testUrls[i].Length, urlBuffer, i * maxLength);
            urlLengths[i] = testUrls[i].Length;
        }

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, testUrls.Length, maxLength, output);

        // Assert
        Assert.Equal(testUrls.Length, output.Count);
        for (int i = 0; i < testUrls.Length; i++)
        {
            Assert.Equal(testUrls[i], output[i]);
        }
    }

    [Fact]
    public void ConvertUrlsToStrings_SpecialCharacters_ConvertsCorrectly()
    {
        // Arrange
        string[] testUrls = new[]
        {
            "https://example.com/path?param=value&special=!@#$%^",
            "https://example.com/path with spaces"
        };

        int maxLength = 100;
        var urlBuffer = new byte[maxLength * testUrls.Length];
        var urlLengths = new int[testUrls.Length];
        var output = new List<string>();

        // Setup test data
        for (int i = 0; i < testUrls.Length; i++)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(testUrls[i]);
            Buffer.BlockCopy(bytes, 0, urlBuffer, i * maxLength, bytes.Length);
            urlLengths[i] = bytes.Length;
        }

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, testUrls.Length, maxLength, output);

        // Assert
        Assert.Equal(testUrls.Length, output.Count);
        for (int i = 0; i < testUrls.Length; i++)
        {
            Assert.Equal(testUrls[i], output[i], ignoreCase: true);
        }
    }

    [Fact]
    public void ConvertUrlsToStrings_MaxLengthBoundary_HandlesCorrectly()
    {
        // Arrange
        const int maxLength = 100;
        string[] testUrls = new[]
        {
            new string('a', maxLength - 1),  // Just under max
            new string('b', maxLength),      // At max
            new string('c', maxLength + 1)   // Over max - should be skipped
        };

        var urlBuffer = new byte[maxLength * testUrls.Length];
        var urlLengths = new int[] { maxLength - 1, maxLength, maxLength + 1 };
        var output = new List<string>();

        // Setup test data
        for (int i = 0; i < testUrls.Length; i++)
        {
            Encoding.ASCII.GetBytes(testUrls[i].AsSpan(0, Math.Min(testUrls[i].Length, maxLength)), 
                urlBuffer.AsSpan(i * maxLength));
        }

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, testUrls.Length, maxLength, output);

        // Assert
        Assert.Single(output);  // Only the first URL should be converted
        Assert.Equal(testUrls[0], output[0]);
    }

    [Fact]
    public void ConvertUrlsToStrings_ZeroLengthUrls_HandlesCorrectly()
    {
        // Arrange
        const int maxLength = 100;
        var urlBuffer = new byte[maxLength * 3];
        var urlLengths = new int[] { 0, 5, 0 };  // Mix of zero and non-zero lengths
        var output = new List<string>();

        // Setup test data
        Encoding.ASCII.GetBytes("test1", urlBuffer.AsSpan(maxLength));  // Only middle URL has content

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, 3, maxLength, output);

        // Assert
        Assert.Single(output);
        Assert.Equal("test1", output[0]);
    }

    [Fact]
    public void ConvertUrlsToStrings_LargeDataSet_HandlesCorrectly()
    {
        // Arrange
        const int urlCount = 1000;
        const int maxLength = 100;
        var random = new Random(42);  // Fixed seed for reproducibility
        var expectedUrls = new List<string>();
        var urlBuffer = new byte[maxLength * urlCount];
        var urlLengths = new int[urlCount];
        var output = new List<string>();

        // Generate random URLs
        for (int i = 0; i < urlCount; i++)
        {
            int length = random.Next(10, 50);  // Random length between 10 and 50
            var url = GenerateRandomUrl(length, random);
            expectedUrls.Add(url);
            
            Encoding.ASCII.GetBytes(url, 0, url.Length, urlBuffer, i * maxLength);
            urlLengths[i] = url.Length;
        }

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, urlCount, maxLength, output);

        // Assert
        Assert.Equal(urlCount, output.Count);
        for (int i = 0; i < urlCount; i++)
        {
            Assert.Equal(expectedUrls[i], output[i]);
        }
    }

    // Helper method to generate random URLs
    private static string GenerateRandomUrl(int length, Random random)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._~:/?#[]@!$&'()*+,;=";
        var result = new StringBuilder(length);
        result.Append("http://");
        for (int i = 7; i < length; i++)
        {
            result.Append(chars[random.Next(chars.Length)]);
        }
        return result.ToString();
    }
}