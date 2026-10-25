using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.BrokenLink;

public record BrokenLinkCrawlDTO
{
    public Guid? Id { get; set; }
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
            BrokenLinks = [.. crawl.BrokenLinks.Select(BrokenLinkDTO.FromBrokenLink)],
            LinksChecked = crawl.LinksChecked,
            Time = crawl.Date
        };
    }
}
