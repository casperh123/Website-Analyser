namespace WebsiteAnalyzer.Core.Entities;

public record Website
{
    public string Url { get; private set; }
    public Guid UserId { get; private set; }
    public IEnumerable<CacheWarm> CacheWarmRuns { get; set; }

    public Website(string url, Guid userId)
    {
        Url = url;
        UserId = userId;
        CacheWarmRuns = [];
    }
}