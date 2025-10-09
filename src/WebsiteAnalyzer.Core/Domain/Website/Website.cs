using WebsiteAnalyzer.Core.Domain.BrokenLink;

namespace WebsiteAnalyzer.Core.Domain.Website;

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
    public string Url { get; private set; }
    public string? Name { get; set; }
    public Guid UserId { get; private set; }
    public ICollection<BrokenLinkCrawl> BrokenLinkCrawls { get; set; }
}