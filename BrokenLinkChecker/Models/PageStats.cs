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
    public string HttpVersion { get; set; }
    public PageHeaders Headers = new PageHeaders();
    public List<string> Scripts = [];
    public List<string> Images = [];

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
    }
}