using WebsiteAnalyzer.Core.DataTransferObject;
using WebsiteAnalyzer.Core.Events;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    event EventHandler<CrawlProgressEventArgs> ProgressUpdated;

    IAsyncEnumerable<BrokenLinkDTO> FindBrokenLinks(string url,
        Guid? crawlId,
        CancellationToken cancellationToken);

    Task<BrokenLinkCrawlDTO> StartCrawl(string url, Guid? userId);
    Task<BrokenLinkCrawlDTO> EndCrawl(BrokenLinkCrawlDTO crawl, int linksChecked, Guid? userId);
    
    Task<ICollection<BrokenLinkCrawlDTO>> GetCrawlsByUserAsync(Guid? userId);
    Task<ICollection<BrokenLinkDTO>> GetBrokenLinksByCrawlIdAsync(Guid crawlId);
}