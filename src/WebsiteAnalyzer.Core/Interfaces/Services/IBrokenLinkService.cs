using Crawl.Models;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    Task<ICollection<BrokenLinkDTO>> FindBrokenLinks(
        Guid websiteId,
        IProgress<CrawlProgress>? progress = null,
        CancellationToken cancellationToken = default
    );

    Task<PagedResponse<BrokenLinkCrawlDTO>> GetCrawlsByWebsiteId(Guid id, int page = 1, int perPage = 20);
    Task<ICollection<BrokenLinkDTO>> GetBrokenLinksByCrawlIdAsync(Guid crawlId);
}
