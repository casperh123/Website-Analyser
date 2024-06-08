namespace BrokenLinkChecker;

public static class Utilities
{
    public static string GetUrl(string baseUrl, string href)
    {
        // Resolve the URL relative to the base URL
        var baseUri = new Uri(baseUrl);
        var resolvedUri = new Uri(baseUri, href);
        return resolvedUri.ToString();
    }
}