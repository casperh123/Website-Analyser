using System.Net;
using WebsiteAnalyzer.Core.Domain.Uptime;

namespace WebsiteAnalyzer.Core.Contracts.Uptime;

record class UptimeStat
{
    public bool Outage { get; }
    public DateTime TimeRecorded { get; }
    public HttpStatusCode? StatusCode { get;  }
    public string? Reason { get; }

    public UptimeStat(
        bool outage,
        DateTime timeRecorded, 
        HttpStatusCode? statusCode, 
        string? reason
        )
    {
        Outage = outage;
        TimeRecorded = timeRecorded;
        StatusCode = statusCode;
        Reason = reason;
    }

    public UptimeStat(DowntimePing ping)
    {
        Outage = true;
        TimeRecorded = ping.TimeRecorded;
        StatusCode = ping.StatusCode;
        Reason = ping.Reason;
    }
}