using WebsiteAnalyzer.Core.DataTransferObject;
using WebsiteAnalyzer.Core.Events;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    event EventHandler<CrawlProgressEventArgs> ProgressUpdated;

    IAsyncEnumerable<BrokenLinkDTO> FindBrokenLinks(string url,
        Guid? userId,
        CancellationToken cancellationToken);
}