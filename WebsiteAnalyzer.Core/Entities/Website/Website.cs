using WebsiteAnalyzer.Core.Entities.BrokenLink;

namespace WebsiteAnalyzer.Core.Entities.Website;

public record Website
{
    public Guid Id { get; private set; }
    public string Url { get; private set; }
    public string? Name { get; set; }
    public Guid UserId { get; private set; }
    public ICollection<CacheWarm> CacheWarmRuns { get; set; }
    public ICollection<BrokenLinkCrawl> BrokenLinkCrawls { get; set; }

    public Website()
    {
        Id = Guid.NewGuid();
        CacheWarmRuns = [];
        BrokenLinkCrawls = [];
    }

    public Website(string url, Guid userId, string? name = null)
    {
        Id = Guid.NewGuid();
        Url = url;
        Name = name ?? Url;
        UserId = userId;
        CacheWarmRuns = [];
        BrokenLinkCrawls = [];
    }
}