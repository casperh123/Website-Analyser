using System.Net;

namespace WebsiteAnalyzer.Core.Domain.Uptime;

public record DownTimePing
{
    public Guid Id { get; set;  }
    public Guid WebsiteId { get; set;  }
    public DateTime TimeRecorded { get; set; }
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