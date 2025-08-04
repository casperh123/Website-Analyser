using WebsiteAnalyzer.Core.Contracts;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Events;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    event EventHandler<CrawlProgressEventArgs> ProgressUpdated;

    Task<BrokenLinkCrawlSession> StreamBrokenLinks(
        string url, 
        Guid? userId,
        IProgress<Progress> progress,
        CancellationToken cancellationToken = default);

    IAsyncEnumerable<BrokenLinkDTO> FindBrokenLinks(string url,
        Guid? crawlId,
        CancellationToken cancellationToken = default);

    Task<BrokenLinkCrawlDTO> StartCrawl(string url, Guid? userId);
    Task<BrokenLinkCrawlDTO> EndCrawl(BrokenLinkCrawlDTO crawl, int linksChecked, Guid? userId);
    
    Task<ICollection<BrokenLinkCrawlDTO>> GetCrawlsByUserAsync(Guid? userId);
    Task<ICollection<BrokenLinkDTO>> GetBrokenLinksByCrawlIdAsync(Guid crawlId);
    Task<ICollection<BrokenLinkCrawlDTO>> GetBrokenLinkCrawlsByUrlAndUserId(string url, Guid userId);
}