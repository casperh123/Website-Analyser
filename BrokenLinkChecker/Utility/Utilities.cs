using System;
using System.Diagnostics;

using BrokenLinkChecker.Models.Headers;

namespace BrokenLinkChecker.utility;

public static class Utilities
{
    public static string GetUrl(string baseUrl, string href)
    {
        if (Uri.TryCreate(baseUrl, UriKind.Absolute, out Uri baseUri) &&
            Uri.TryCreate(baseUri, href, out Uri resultUri))
        {
            return resultUri.ToString();
        }
        return href;
    }

    
    public static bool IsAsyncOrFragmentRequest(string url)
    {
        // File extensions that should not be classified as async or fragment
        string[] excludedExtensions = { ".js", ".css" };

        // Parse the URL to extract the path without query parameters
        if (Uri.TryCreate(url, UriKind.Absolute, out Uri uri))
        {
            string path = uri.LocalPath;

            // Check if the path ends with any of the excluded extensions
            if (excludedExtensions.Any(extension => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase)))
            {
                return false;
            }
        }

        // Check for async-related keywords or fragment/query parts in the URL
        string[] asyncKeywords = { "ajax", "async", "action=async" };
        if (asyncKeywords.Any(keyword => url.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
            
        return url.Contains('#') || url.Contains('?') || url.Contains("mailto");
    }
    
    public static async Task<(T, long)> BenchmarkAsync<T>(Func<Task<T>> function)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        T result = await function.Invoke();

        return (result, stopwatch.ElapsedMilliseconds);
    }
}