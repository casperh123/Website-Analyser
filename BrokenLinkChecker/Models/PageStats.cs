using System.Net;
using BrokenLinkChecker.Models.Headers;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.models;

public class PageStats
{
    public string Url;
    public HttpStatusCode StatusCode;
    public long ResponseTime;
    public long DocumentParseTime;
    public long CombinedTime => ResponseTime + DocumentParseTime;
    public PageHeaders Headers = new PageHeaders();

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
        Headers = new PageHeaders(response.Headers, response.Content.Headers);
    }
}