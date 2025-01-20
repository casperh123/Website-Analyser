using BrokenLinkChecker.Models.Links;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Events;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    event EventHandler<CrawlProgressEventArgs> ProgressUpdated;

    IAsyncEnumerable<IndexedLink> FindBrokenLinks(string url,
        Guid? userId,
        CancellationToken cancellationToken);
}