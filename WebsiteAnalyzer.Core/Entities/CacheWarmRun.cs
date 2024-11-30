namespace WebsiteAnalyzer.Core.Entities;

public record CacheWarmRun
{
    public Guid Id { get; init; }
    public int VisitedPages { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public Guid? WebsiteId { get; set; }
    public Website? Website { get; set; }
}