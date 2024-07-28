namespace BrokenLinkChecker.utility;

public static class Utilities
{
    public static string GetUrl(string baseUrl, string href)
    {
        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(href))
        {
            return href; // Optionally, return null or throw an exception based on your error handling strategy
        }

        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri) &&
            Uri.TryCreate(baseUri, href, out Uri resultUri))
        {
            // UriBuilder is used to normalize and encode the URL properly.
            return new UriBuilder(resultUri).Uri.AbsoluteUri;
        }
        return href; // Consider logging this case as it indicates a potential issue with base or relative URL inputs.
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