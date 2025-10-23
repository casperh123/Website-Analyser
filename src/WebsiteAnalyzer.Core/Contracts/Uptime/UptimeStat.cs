using System.Net;
using WebsiteAnalyzer.Core.Domain.Uptime;

namespace WebsiteAnalyzer.Core.Contracts.Uptime;

public record UptimeStat
{
    public int Outages { get; }
    public DateTime TimeRecorded { get; }
    public HttpStatusCode? StatusCode { get;  }
    public string? Reason { get; }

    public UptimeStat(
        int outages,
        DateTime timeRecorded, 
        HttpStatusCode? statusCode, 
        string? reason
        )
    {
        Outages = outages;
        TimeRecorded = timeRecorded;
        StatusCode = statusCode;
        Reason = reason;
    }
}