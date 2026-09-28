using WebsiteAnalyzer.Core.Domain.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Website;

public record WebsiteDTO
{
    public Guid Id { get; private set; }
    public string Url { get; set; }
    public string? Name { get; set; }
    public Guid UserId { get; set; }
    public ICollection<BrokenLinkCrawl> BrokenLinkCrawls { get; set; }

    public WebsiteDTO From(Domain.Website.Website website)
    {
        return new WebsiteDTO
        {
            Id = website.Id,
            Url = website.Url,
            Name = website.Name,
            UserId = website.UserId,
            BrokenLinkCrawls = website.BrokenLinkCrawls
        };
    }
}