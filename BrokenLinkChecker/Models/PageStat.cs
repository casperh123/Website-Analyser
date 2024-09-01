using System.Net;
using BrokenLinkChecker.Models.Headers;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.models;

public record PageStat
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

    public PageStat(string url, HttpStatusCode statusCode, long responseTime = 0, long documentParseTime = 0)
    {
        Url = url;
        StatusCode = statusCode;
        ResponseTime = responseTime;
        DocumentParseTime = documentParseTime;
    }
    
    public PageStat(string url, HttpResponseMessage response, ResourceType type, long requestTime = 0, long parseTime = 0)
    {
        Url = url;
        ResponseTime = requestTime;
        DocumentParseTime = parseTime;
        Size = response.Content.Headers.ContentLength ?? 0;
        SizeKb = Size / 1024f;
        StatusCode = response.StatusCode;
        HttpVersion = response.Version.ToString();
        Headers = new PageHeaders(response.Headers, response.Content.Headers);
        Type = type;
    }
}
