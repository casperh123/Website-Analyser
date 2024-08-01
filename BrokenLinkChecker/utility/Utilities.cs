using System.Diagnostics;
using BrokenLinkChecker.models;

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

    public static (T, long) Benchmark<T>(Func<T> function)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        T result = function.Invoke();

        return (result, stopwatch.ElapsedMilliseconds);
    }
    
    public static async Task<(T, long)> BenchmarkAsync<T>(Func<Task<T>> function)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();

        T result = await function.Invoke();

        return (result, stopwatch.ElapsedMilliseconds);
    }
    
    public static PageHeaders AddRequestHeaders(HttpResponseMessage response)
    {
        if (response == null) throw new ArgumentNullException(nameof(response));

        return new PageHeaders
        {
            CacheControl = response.Headers.CacheControl?.ToString() ?? "",
            CacheStatus = response.Headers.TryGetValues("x-cache", out var cacheStatus) ? string.Join(", ", cacheStatus) : "",
            ContentEncoding = response.Content.Headers.ContentEncoding?.FirstOrDefault() ?? "",
            LastModified = response.Content.Headers.LastModified?.ToString() ?? "",
            Server = response.Headers.Server?.ToString() ?? ""
        };
    }
}