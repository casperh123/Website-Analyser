namespace WebsiteAnalyzer.Core.Entities;

public record CacheWarm
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public int VisitedPages { get; set; }
    public DateTime StartTimeUtc { get; private set; }
    public DateTime EndTime { get; private set; }
    public int AveragePageTime => (int)(VisitedPages > 0 ? TotalTime.TotalMilliseconds / VisitedPages : 0);
    public bool IsCompleted => EndTime != DateTime.MinValue;
    public TimeSpan TotalTime => IsCompleted ? (EndTime - StartTimeUtc) : TimeSpan.Zero;
    public DateTime StartTimeLocal => StartTimeUtc.ToLocalTime();

    

    public void SetStartTime()
    {
        StartTimeUtc = DateTime.UtcNow;
    }

    public void SetEndTime()
    {
        EndTime = DateTime.UtcNow;
    }
    
    public CacheWarm()
    {
        StartTimeUtc = DateTime.MinValue.ToUniversalTime();
        EndTime = DateTime.MinValue.ToUniversalTime();
    }

    public CacheWarm(Website.Website website)
    {
        Id = Guid.NewGuid();
        WebsiteId = website.Id;
    }
}