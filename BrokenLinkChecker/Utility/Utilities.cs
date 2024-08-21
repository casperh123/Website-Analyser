using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using BrokenLinkChecker.models;

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
        string[] asyncKeywords = { "ajax", "async", "action=async" };

        if (asyncKeywords.Any(keyword => url.Contains(keyword, StringComparison.OrdinalIgnoreCase)))
        {
            return true;
        }
        
        return url.Contains('#') || url.Contains('?');
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