using BrokenLinkChecker.Models.Links;
using WebsiteAnalyzer.Core.Entities.BrokenLink;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    IAsyncEnumerable<IndexedLink> FindBrokenLinks(string url,
        Guid? userId,
        Action<int>? onLinkEnqueued,
        Action<int>? onLinkChecked,
        CancellationToken cancellationToken);
}