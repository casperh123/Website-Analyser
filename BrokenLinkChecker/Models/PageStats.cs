using System.Net;
using System.Net.Http.Headers;

namespace BrokenLinkChecker.models;

public class PageStats(string url, HttpStatusCode statusCode, long responseTime = 0, long documentParseTime = 0, PageHeaders headers = null)
{
    public string Url = url;
    public HttpStatusCode StatusCode = statusCode;
    public long ResponseTime = responseTime;
    public long DocumentParseTime = documentParseTime;
    public long CombinedTime => ResponseTime + DocumentParseTime;
    public PageHeaders? Headers = headers ?? new PageHeaders();
}