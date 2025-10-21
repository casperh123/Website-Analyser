using System.Net;

namespace WebsiteAnalyzer.Core.Domain.Uptime;

public record DownTimePing
{
    public Guid Id { get; }
    public Guid WebsiteId { get; }
    public DateTime TimeRecorded { get; }
    public HttpStatusCode? StatusCode { get; set;  }
    public string? Reason { get; set;  }

    public DownTimePing() {}
    
    public DownTimePing(
        Guid websiteId
    )
    {
        Id = Guid.NewGuid();
        WebsiteId = websiteId;
        TimeRecorded = DateTime.Now;
    }
}