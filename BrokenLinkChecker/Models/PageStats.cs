using System.Net;
using BrokenLinkChecker.Models.Headers;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.models;

public record PageStats
{
    public string Url;
    public HttpStatusCode StatusCode;
    public long ResponseTime;
    public long DocumentParseTime;
    public long CombinedTime => ResponseTime + DocumentParseTime;
    public int Size;
    public string HttpVersion { get; set; }
    public PageHeaders Headers = new PageHeaders();
    public ResourceType Type { get; set; }

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
        StatusCode = response.StatusCode;
        HttpVersion = response.Version.ToString();
        Headers = new PageHeaders(response.Headers, response.Content.Headers);
        Type = DetermineResourceType(response.Content.Headers.ContentType?.MediaType);
    }

    private ResourceType DetermineResourceType(string? mediaType)
    {
        if (string.IsNullOrEmpty(mediaType))
        {
            return ResourceType.Resource; // Default to general resource if content type is unknown
        }

        if (mediaType.StartsWith("text/html", StringComparison.OrdinalIgnoreCase))
        {
            return ResourceType.Page;
        }
        if (mediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return ResourceType.Image;
        }
        if (mediaType.StartsWith("application/javascript", StringComparison.OrdinalIgnoreCase) ||
            mediaType.StartsWith("text/javascript", StringComparison.OrdinalIgnoreCase))
        {
            return ResourceType.Script;
        }
        if (mediaType.StartsWith("text/css", StringComparison.OrdinalIgnoreCase))
        {
            return ResourceType.Stylesheet;
        }

        // Add more content types as needed
        return ResourceType.Resource; // Fallback to generic resource
    }
}
