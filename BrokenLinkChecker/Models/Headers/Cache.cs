using System.Net.Http.Headers;

namespace BrokenLinkChecker.Models.Headers;

public record Cache
{
    public string Age { get; set; } = string.Empty;
    public string CacheControl { get; set; } = string.Empty;
    public string CacheStatus { get; set; } = "UNKNOWN";
    public Dictionary<string, string> CacheHeaders { get; set; } = new Dictionary<string, string>();

    public Cache()
    {
    }
    
    public Cache(HttpResponseHeaders headers)
    {
        Age = headers.Age?.ToString() ?? string.Empty;
        CacheControl = headers.CacheControl?.ToString() ?? string.Empty;
        CacheHeaders = new Dictionary<string, string>();

        // Determine Cache Status
        if (headers.Age.HasValue && headers.Age.Value.TotalSeconds > 5)
        {
            CacheStatus = "HIT";
        }
        else if (headers.CacheControl != null && 
                 (headers.CacheControl.NoCache || headers.CacheControl.NoStore || headers.CacheControl.Private))
        {
            CacheStatus = "BYPASS";
        }
        else
        {
            CacheStatus = "UNKNOWN";
        }

        // Populate other cache-related headers
        foreach (KeyValuePair<string, IEnumerable<string>> header in headers)
        {
            if (header.Key.Contains("cache", StringComparison.OrdinalIgnoreCase))
            {
                CacheHeaders[header.Key] = string.Join(", ", header.Value);
            }
        }
    }
}