using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Entities;

public record CrawlSchedule
{
    public Guid UserId { get; set; }
    public string WebsiteUrl { get; set; }
    public DateTime LastCrawlDate { get; set; }
    public Frequency Frequency { get; set; }
    public CrawlAction Action { get; set; }
    public Status Status { get; set; }
    
}