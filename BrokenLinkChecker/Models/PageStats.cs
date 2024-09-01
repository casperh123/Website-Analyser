using System.Net;
using BrokenLinkChecker.Models.Headers;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.models;

public record PageStats
{
    public string Url { get; init; }
    public HttpStatusCode StatusCode { get; private set; }
    public long ResponseTime { get; private set; }
    public long DocumentParseTime { get; private set; }
    public long CombinedTime => ResponseTime + DocumentParseTime;
    public long Size { get; private set; }
    public float SizeKb { get; private set; }
    public string HttpVersion { get; private set; } = string.Empty;
    public PageHeaders Headers { get; private set; } = new PageHeaders();
    public ResourceType Type { get; private set; }

    public PageStats(string url, HttpStatusCode statusCode, long responseTime = 0, long documentParseTime = 0)
    {
        Url = url;
        StatusCode = statusCode;
        ResponseTime = responseTime;
        DocumentParseTime = documentParseTime;
    }

    public void AddMetrics(HttpResponseMessage response, long requestTime = 0, long parseTime = 0)
    {
        ResponseTime = requestTime;
        DocumentParseTime = parseTime;
        Size = response.Content.Headers.ContentLength ?? 0;
        SizeKb = Size / 1024f;
        StatusCode = response.StatusCode;
        HttpVersion = response.Version.ToString();
        Headers = new PageHeaders(response.Headers, response.Content.Headers);
        Type = DetermineResourceType(response.Content.Headers.ContentType?.MediaType);
    }

    private static ResourceType DetermineResourceType(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return ResourceType.Resource;
        }

        return mediaType.ToLowerInvariant() switch
        {
            { } m when m.StartsWith("text/html") => ResourceType.Page,
            { } m when m.StartsWith("image/") => ResourceType.Image,
            { } m when m.StartsWith("application/javascript") || m.StartsWith("text/javascript") => ResourceType.Script,
            { } m when m.StartsWith("text/css") => ResourceType.Stylesheet,
            _ => ResourceType.Resource
        };
    }
}
