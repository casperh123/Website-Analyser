namespace BrokenLinkChecker.utility;

public static class Utilities
{
    public static Uri GetUrl(string baseUrl, string href)
    {
        // Resolve the URL relative to the base URL
        Uri baseUri = new Uri(baseUrl);
        Uri resolvedUri = new Uri(baseUri, href);
        return resolvedUri;
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