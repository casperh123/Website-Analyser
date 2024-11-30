namespace WebsiteAnalyzer.Core.Entities;

public record Website
{
    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Url { get; init; }
    public IEnumerable<CacheWarmRun> CacheWarmRuns { get; set; }
}