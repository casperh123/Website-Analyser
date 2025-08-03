using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Entities.BrokenLink;

namespace WebsiteAnalyzer.Core.Contracts;

public record BrokenLinkCrawlDTO
{
    public Guid? Id { get; set; }
    public string Url { get; set; }
    public DateTime Time { get; set; }
    public int LinksChecked { get; set; }
    public ICollection<BrokenLinkDTO> BrokenLinks { get; set; } = [];

    public BrokenLinkCrawlDTO() {}
    
    public BrokenLinkCrawlDTO(string url)
    {
        Id = Guid.NewGuid();
        Url = url;
        Time = DateTime.UtcNow;
        BrokenLinks = [];
    }

    public BrokenLinkCrawlDTO(Guid id, string url, DateTime time)
    {
        Id = id;
        Url = url;
        Time = time;
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
    
    public BrokenLinkCrawl ToBrokenLink(Guid userId) {
         return new BrokenLinkCrawl
         {
             Id = Id.Value,
             UserId = userId,
             Url = Url,
             BrokenLinks = BrokenLinks.Select(link => link.ToBrokenLink()).ToList(),
             LinksChecked = LinksChecked,
             Date = Time
         };
    }
}