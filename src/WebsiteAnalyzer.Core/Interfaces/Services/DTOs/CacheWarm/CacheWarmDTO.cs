namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.CacheWarm;

public record CacheWarmDTO
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public int VisitedPages { get; set; }
    public DateTime StartTimeUtc { get; set; }
    public DateTime EndTime { get; set; }
    public int AveragePageTime { get; set; }
    public bool IsCompleted { get; set; }
    public TimeSpan TotalTime { get; set; }
    public DateTime StartTimeLocal { get; set; }

    public static CacheWarmDTO From(Domain.CacheWarm cacheWarm)
    {
        return new CacheWarmDTO
        {
            Id = cacheWarm.Id,
            WebsiteId = cacheWarm.WebsiteId,
            VisitedPages = cacheWarm.VisitedPages,
            StartTimeLocal = cacheWarm.StartTimeLocal,
            StartTimeUtc = cacheWarm.StartTimeUtc,
            EndTime = cacheWarm.EndTime,
            AveragePageTime = cacheWarm.AveragePageTime,
            IsCompleted = cacheWarm.IsCompleted,
            TotalTime = cacheWarm.TotalTime
        };
    }
}