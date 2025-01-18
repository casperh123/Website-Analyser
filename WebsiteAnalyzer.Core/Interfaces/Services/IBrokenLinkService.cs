using BrokenLinkChecker.Models.Links;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IBrokenLinkService
{
    Task FindBrokenLinks(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked,
        Action<IndexedLink> onLinkFound, CancellationToken cancellationToken);
}