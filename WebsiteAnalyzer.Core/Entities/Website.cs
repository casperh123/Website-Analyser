namespace WebsiteAnalyzer.Core.Entities;

public record Website
{
    public string Url { get; init; }
    public IEnumerable<CacheWarm> CacheWarmRuns { get; set; }
    public Guid UserId { get; set; }
}