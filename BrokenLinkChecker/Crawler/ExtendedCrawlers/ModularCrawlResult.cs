using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawlResult<T> where T : Link
{
    public int LinksChecked { get; private set; }
    public int LinksEnqueued { get; private set; }

    public event Action<T> OnResouceVisited;
    public event Action<int> OnLinksEnqueued;
    public event Action<int> OnLinksChecked;

    public void AddResource(T resource)
    {
        OnResouceVisited.Invoke(resource);
    }

    public void IncrementLinksChecked()
    {
        LinksChecked++;
        OnLinksChecked?.Invoke(LinksChecked);
    }

    public void SetLinksEnqueued(int count)
    {
        LinksEnqueued = count;
        OnLinksEnqueued?.Invoke(LinksEnqueued);
    }
}