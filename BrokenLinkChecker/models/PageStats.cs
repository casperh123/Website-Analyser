using System.Net;

namespace BrokenLinkChecker.models;

public class PageStats
{
    public HttpStatusCode StatusCode;
    public long ResponseTime;
    public long DocumentParseTime;

    public PageStats(HttpStatusCode statusCode, long responseTime = 0, long documentParseTime = 0)
    {
        StatusCode = statusCode;
        ResponseTime = responseTime;
        DocumentParseTime = documentParseTime;
    }
}