namespace WebsiteAnalyzer.Core.Entities.BrokenLink;

public record BrokenLinkCrawl
{
    public BrokenLinkCrawl(Guid id, Guid userId, string url)
    {
        Id = id;
        UserId = userId;
        Url = url;
        BrokenLinks = [];
    }

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Url { get; set; }
    public List<BrokenLink> BrokenLinks { get; set; }
    
    public void AddBrokenLink(BrokenLink brokenLink) => BrokenLinks.Add(brokenLink);
}