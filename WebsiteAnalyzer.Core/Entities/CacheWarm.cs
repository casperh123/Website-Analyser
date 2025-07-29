namespace WebsiteAnalyzer.Core.Entities;

public record CacheWarm
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public int VisitedPages { get; set; }
    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public bool IsCompleted => EndTime != DateTime.MinValue;
    public TimeSpan TotalTime => IsCompleted ? (EndTime - StartTime) : TimeSpan.Zero;
    

    public void SetStartTime()
    {
        StartTime = DateTime.UtcNow;
    }

    public void SetEndTime()
    {
        EndTime = DateTime.UtcNow;
    }
    
    public CacheWarm()
    {
        StartTime = DateTime.MinValue.ToUniversalTime();
        EndTime = DateTime.MinValue.ToUniversalTime();
    }

    public CacheWarm(Website.Website website)
    {
        Id = Guid.NewGuid();
        WebsiteId = website.Id;
    }
}