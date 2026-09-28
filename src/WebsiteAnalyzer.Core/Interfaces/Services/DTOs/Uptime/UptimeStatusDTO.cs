using System.Net;

namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Uptime;

public record UptimeStatusDTO
{
    public int Outages { get; }
    public DateTime TimeRecorded { get; }
    public HttpStatusCode? StatusCode { get;  }
    public string? Reason { get; }

    public UptimeStatusDTO(
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