namespace WebsiteAnalyzer.Core.Domain.BrokenLink;

public record BrokenLinkCrawl
{
    public BrokenLinkCrawl()
    {
        
    }
    
    public BrokenLinkCrawl(Guid userId, string url)
    {
        Id = Guid.NewGuid();
        UserId = userId;
        Url = url;
        BrokenLinks = [];
        Date = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Url { get; set; }
    public ICollection<Domain.BrokenLink.BrokenLink> BrokenLinks { get; set; } = [];
    public int LinksChecked { get; set; }
    public DateTime Date { get; set; }
    
    public void AddBrokenLink(Domain.BrokenLink.BrokenLink brokenLink) => BrokenLinks.Add(brokenLink);
}