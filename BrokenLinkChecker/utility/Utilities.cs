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
}