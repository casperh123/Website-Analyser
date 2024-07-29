using System.Net;

namespace BrokenLinkChecker.models;

public class PageStats(string url, HttpStatusCode statusCode, long responseTime = 0, long documentParseTime = 0)
{
    public string Url = url;
    public HttpStatusCode StatusCode = statusCode;
    public long ResponseTime = responseTime;
    public long DocumentParseTime = documentParseTime;
}