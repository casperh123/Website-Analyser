namespace WebsiteAnalyzer.Core.Entities;

public record CacheWarm
{
    public required Guid Id { get; init; }
    public required Guid? UserId { get; init; }
    public required string WebsiteUrl { get; init; }
    public int VisitedPages { get; set; }
    
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public CrawlSchedule? Schedule { get; init; }
    
    public bool IsCompleted => EndTime != DateTime.MinValue;
    public TimeSpan TotalTime => IsCompleted ? (EndTime - StartTime) : TimeSpan.Zero;

    public void Start()
    {
        StartTime = DateTime.UtcNow;
    }

    public void Complete()
    {
        EndTime = DateTime.UtcNow;
    }
    
    public CacheWarm()
    {
        StartTime = DateTime.MinValue.ToUniversalTime();
        EndTime = DateTime.MinValue.ToUniversalTime();
    }
}