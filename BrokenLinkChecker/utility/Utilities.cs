namespace BrokenLinkChecker.utility;

public static class Utilities
{
    public static string GetUrl(string baseUrl, string href)
    {
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out var baseUri))
        {
            if (Uri.TryCreate(baseUri, href, out var resultUri))
            {
                // This will ensure the URL is properly encoded
                return new UriBuilder(resultUri).Uri.AbsoluteUri;
            }
        }
        return href;
    }
    
    public static bool IsAsyncOrFragmentRequest(string url)
    {
        string[] asyncKeywords = { "ajax", "async", "action=async" };

        if (asyncKeywords.Any(keyword => url.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        // Check if the URL is a fragment identifier (e.g., #section)
        return url.Contains('#') || url.Contains('?');
    }
}