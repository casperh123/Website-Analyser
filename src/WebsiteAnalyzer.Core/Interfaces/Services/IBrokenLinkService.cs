using Crawl.Models;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    Task<ICollection<BrokenLinkDTO>> FindBrokenLinks(
        Website website,
        IProgress<CrawlProgress>? progress = null,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<BrokenLinkCrawlDTO>> GetCrawlsByUserAsync(Guid? userId);
    Task<ICollection<BrokenLinkDTO>> GetBrokenLinksByCrawlIdAsync(Guid crawlId);
    Task<ICollection<BrokenLinkCrawlDTO>> GetBrokenLinkCrawlsByUrlAndUserId(string url, Guid userId);
}