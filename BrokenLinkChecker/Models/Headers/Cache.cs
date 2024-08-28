using System.Net.Http.Headers;

namespace BrokenLinkChecker.Models.Headers;

public record Cache
{
    public string CacheStatus = "";
    public string CacheControl = "";
    public string Age { get; set; } = "";
    public Dictionary<string, string> CacheHeaders { get; set; }

    public Cache()
    {
    }
    
    public Cache(HttpResponseHeaders headers)
    {
        CacheControl = headers.CacheControl?.ToString() ?? "";
        CacheStatus = headers.TryGetValues("x-cache", out var cacheStatus) ? string.Join(", ", cacheStatus) : "";
    }
}