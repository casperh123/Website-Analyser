using System.Net;
using BrokenLinkChecker.Models.Headers;
using BrokenLinkChecker.utility;

namespace BrokenLinkChecker.models;

public class PageStats(string url, HttpStatusCode statusCode, long responseTime = 0, long documentParseTime = 0)
{
    public string Url = url;
    public HttpStatusCode StatusCode = statusCode;
    public long ResponseTime = responseTime;
    public long DocumentParseTime = documentParseTime;
    public long CombinedTime => ResponseTime + DocumentParseTime;
    public PageHeaders? Headers;
    
    
    public void AddMetrics(HttpResponseMessage response, long requestTime = 0, long parseTime = 0)
    {
        ResponseTime = requestTime;
        DocumentParseTime = parseTime;
        StatusCode = response.StatusCode;
        Headers = new PageHeaders(response.Headers);
    }
}