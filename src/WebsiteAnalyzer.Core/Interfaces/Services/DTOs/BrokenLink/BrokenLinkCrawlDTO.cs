using WebsiteAnalyzer.Core.Domain.BrokenLink;

namespace WebsiteAnalyzer.Core.Contracts.BrokenLink;

public record BrokenLinkCrawlDTO
{
    public Guid? Id { get; set; }
    public string Url { get; set; }
    public DateTime LocalTime => Time.ToLocalTime();
    public int LinksChecked { get; set; }
    public ICollection<BrokenLinkDTO> BrokenLinks { get; set; } = [];
    private DateTime Time { get; set; }
    
    public BrokenLinkCrawlDTO()
    {
    }

    public static BrokenLinkCrawlDTO From(BrokenLinkCrawl crawl)
    {
        return new BrokenLinkCrawlDTO
        {
            Id = crawl.Id,
            Url = crawl.Url,
            BrokenLinks = crawl.BrokenLinks.Select(BrokenLinkDTO.FromBrokenLink).ToList(),
            LinksChecked = crawl.LinksChecked,
            Time = crawl.Date
        };
    }
}