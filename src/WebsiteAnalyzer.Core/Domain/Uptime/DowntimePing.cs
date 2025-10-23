using System.Net;

namespace WebsiteAnalyzer.Core.Domain.Uptime;

public record DowntimePing
{
    public Guid Id { get; set;  }
    public Guid WebsiteId { get; set;  }
    public DateTime TimeRecorded { get; set; }
    public HttpStatusCode? StatusCode { get; set;  }
    public string? Reason { get; set;  }

    public DowntimePing() {}

    public DowntimePing(
        Guid websiteId,
        DateTime timeRecorded
    )
    {
        Id = Guid.NewGuid();
        WebsiteId = websiteId;
        TimeRecorded = timeRecorded;
    }
    
    public DowntimePing(
        Guid websiteId
    )
    {
        Id = Guid.NewGuid();
        WebsiteId = websiteId;
        TimeRecorded = DateTime.UtcNow;
    }
}