namespace WebsiteAnalyzer.Core.Entities;

public record CacheWarm
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string WebsiteUrl { get; init; }
    public int VisitedPages { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public CrawlSchedule? Schedule { get; set; }
    public bool IsCompleted => EndTime != DateTime.MinValue;
    public TimeSpan TotalTime => IsCompleted ? (EndTime - StartTime) : TimeSpan.Zero;
}