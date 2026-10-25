
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Domain.OrderChecks;
using WebsiteAnalyzer.Core.Domain.Uptime;

namespace WebsiteAnalyzer.Core.Domain;

public record Website
{
    public Website()
    {
        Id = Guid.NewGuid();
        Url = "";
        BrokenLinkCrawls = [];
    }

    public Website(string url, Guid userId, string? name = null)
    {
        Id = Guid.NewGuid();
        Url = url;
        Name = name ?? Url;
        UserId = userId;
        BrokenLinkCrawls = [];
    }

    public Guid Id { get; private set; }
    public string Url { get; set; }
    public string? Name { get; set; }
    public Guid UserId { get; set; }
    public OrderCheckKeys OrderCheckKeys { get; set; }
    public ICollection<BrokenLinkCrawl> BrokenLinkCrawls { get; set; }
    public ICollection<CacheWarm> CacheWarms { get; set; }
    public ICollection<ScheduledAction> ScheduledActions  { get; set; }
    public ICollection<DowntimePing> DowntimePings { get; set; }
    public ICollection<OrderCheck> OrderChecks { get; set; }
}
