using WebsiteAnalyzer.Core.Domain.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Website;

public record WebsiteDTO
{
    public required Guid Id { get; set; }
    public required string Url { get; set; }
    public required string? Name { get; set; }
    public required Guid UserId { get; set; }
    public required ICollection<BrokenLinkCrawl> BrokenLinkCrawls { get; set; }

    public static WebsiteDTO From(Domain.Website.Website website)
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