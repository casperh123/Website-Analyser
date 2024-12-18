using System.Text;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction.FastParse;

public class StringCreatorUTF8Tests
{
    [Fact]
    public void ConvertUrlsToStrings_SpecialCharacters_ConvertsCorrectly()
    {
        // Arrange
        string[] testUrls = new[]
        {
            "https://example.com/path?param=value&special=!@#$%^",
            "https://test.org/résumé",
            "https://example.com/path with spaces"
        };

        int maxLength = 200;  // Increased to handle UTF-8 encoded characters
        var urlBuffer = new byte[maxLength * testUrls.Length];
        var urlLengths = new int[testUrls.Length];
        var output = new List<string>();

        // Setup test data - using UTF8 encoding
        var encoding = Encoding.UTF8;
        for (int i = 0; i < testUrls.Length; i++)
        {
            // Get bytes with UTF8 encoding
            byte[] bytes = encoding.GetBytes(testUrls[i]);
            // Copy to buffer
            Buffer.BlockCopy(bytes, 0, urlBuffer, i * maxLength, bytes.Length);
            // Store the actual byte length
            urlLengths[i] = bytes.Length;
        }

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, testUrls.Length, maxLength, output);

        // Assert
        Assert.Equal(testUrls.Length, output.Count);
        for (int i = 0; i < testUrls.Length; i++)
        {
            Assert.Equal(testUrls[i], output[i], StringComparer.Ordinal);
        }
    }

    // Additional comprehensive tests for edge cases
    [Fact]
    public void ConvertUrlsToStrings_MultiByteCharacters_ConvertsCorrectly()
    {
        // Arrange
        string[] testUrls = new[]
        {
            "https://example.com/编程",  // Chinese
            "https://example.com/プログラミング",  // Japanese
            "https://example.com/한글",  // Korean
            "https://example.com/🌍",    // Emoji
            "https://example.com/üößäÄ"  // German
        };

        int maxLength = 200;
        var urlBuffer = new byte[maxLength * testUrls.Length];
        var urlLengths = new int[testUrls.Length];
        var output = new List<string>();
        var encoding = Encoding.UTF8;

        for (int i = 0; i < testUrls.Length; i++)
        {
            byte[] bytes = encoding.GetBytes(testUrls[i]);
            Buffer.BlockCopy(bytes, 0, urlBuffer, i * maxLength, bytes.Length);
            urlLengths[i] = bytes.Length;
        }

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, testUrls.Length, maxLength, output);

        // Assert
        Assert.Equal(testUrls.Length, output.Count);
        for (int i = 0; i < testUrls.Length; i++)
        {
            Assert.Equal(testUrls[i], output[i], StringComparer.Ordinal);
        }
    }

    [Theory]
    [InlineData(50)]     // Small URL
    [InlineData(100)]    // Medium URL
    [InlineData(200)]    // Long URL
    public void ConvertUrlsToStrings_VariousLengths_WithSpecialChars_ConvertsCorrectly(int baseLength)
    {
        // Arrange
        string specialChars = "éüößäÄ";
        string baseUrl = "https://test.org/";
        
        // Ensure baseLength is at least long enough for the base URL + special chars
        int minLength = baseUrl.Length + specialChars.Length;
        int actualLength = Math.Max(baseLength, minLength);
        int paddingLength = actualLength - baseUrl.Length - specialChars.Length;
        
        // Create padding only if we need it
        string padding = paddingLength > 0 ? new string('a', paddingLength) : "";
        string testUrl = baseUrl + padding + specialChars;

        // Calculate the required buffer size based on max possible UTF-8 bytes per character
        int maxBytesPerChar = 4;  // UTF-8 can use up to 4 bytes per character
        int maxByteLength = testUrl.Length * maxBytesPerChar;
        
        var urlBuffer = new byte[maxByteLength];
        var urlLengths = new int[1];
        var output = new List<string>();
        var encoding = Encoding.UTF8;

        // Get actual byte length
        byte[] bytes = encoding.GetBytes(testUrl);
        Buffer.BlockCopy(bytes, 0, urlBuffer, 0, bytes.Length);
        urlLengths[0] = bytes.Length;

        // Act
        UrlCreater.ConvertUrlsToStrings(urlBuffer, urlLengths, 1, maxByteLength, output);

        // Assert
        Assert.Single(output);
        Assert.Equal(testUrl, output[0], StringComparer.Ordinal);
    }

}