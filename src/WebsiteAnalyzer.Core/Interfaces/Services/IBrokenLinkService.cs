using WebsiteAnalyzer.Core.Contracts;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    IAsyncEnumerable<BrokenLinkDTO> FindBrokenLinks(
        Website website,
        IProgress<Progress>? progress = null,
        CancellationToken cancellationToken = default
    );

    IAsyncEnumerable<BrokenLinkDTO> FindBrokenLinksAnonymus(
        string url,
        IProgress<Progress>? progress = null,
        CancellationToken cancellationToken = default
    );

    Task<ICollection<BrokenLinkCrawlDTO>> GetCrawlsByUserAsync(Guid? userId);
    Task<ICollection<BrokenLinkDTO>> GetBrokenLinksByCrawlIdAsync(Guid crawlId);
    Task<ICollection<BrokenLinkCrawlDTO>> GetBrokenLinkCrawlsByUrlAndUserId(string url, Guid userId);
}