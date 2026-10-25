namespace WebsiteAnalyzer.Core.Domain.BrokenLink;

public record BrokenLinkCrawl
{
    public BrokenLinkCrawl()
    {
    }

    public BrokenLinkCrawl(Website website)
    {
        Id = Guid.NewGuid();
        WebsiteId = website.Id;
        Website = website;
        BrokenLinks = [];
        Date = DateTime.UtcNow;
    }

    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public Website Website { get; set; }
    public ICollection<BrokenLink> BrokenLinks { get; set; } = [];
    public int LinksChecked { get; set; }
    public DateTime Date { get; set; }

    public void AddBrokenLink(BrokenLink brokenLink)
    {
        BrokenLinks.Add(brokenLink);
    }
}
