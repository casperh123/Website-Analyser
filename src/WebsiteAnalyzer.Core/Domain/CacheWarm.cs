namespace WebsiteAnalyzer.Core.Domain;

public record CacheWarm
{
    public CacheWarm() { }

    public CacheWarm(Website.Website website)
    {
        Id = Guid.NewGuid();
        WebsiteId = website.Id;
    }

    public CacheWarm(
        Website.Website website,
        int linksChecked,
        DateTime startTime,
        DateTime endTime
    )
    {
        Id = Guid.NewGuid();
        WebsiteId = website.Id;
        VisitedPages = linksChecked;
        StartTimeUtc = startTime;
        EndTime = endTime;
    }

    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public int VisitedPages { get; set; }
    public DateTime StartTimeUtc { get; private set; }
    public DateTime EndTime { get; private set; }
    public int AveragePageTime => (int)(VisitedPages > 0 ? TotalTime.TotalMilliseconds / VisitedPages : 0);
    public bool IsCompleted => EndTime != DateTime.MinValue;
    public TimeSpan TotalTime => IsCompleted ? EndTime - StartTimeUtc : TimeSpan.Zero;
    public DateTime StartTimeLocal => StartTimeUtc.ToLocalTime();
}