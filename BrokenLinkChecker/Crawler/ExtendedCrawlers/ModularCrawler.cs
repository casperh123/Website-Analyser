using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T> where T : Link
{
    private readonly ILinkProcessor<T> _linkProcessor;
    private HashSet<string> _visitedSites;
    private HashSet<string> _enqueuedSites;

    private int LinksChecked { get; set; }
    private int LinksEnqueued { get; set; }
    
    private const int DefaultQueueSize = 1000;
    
    public event Action<T>? OnResourceVisited;
    public event Action<int>? OnLinksEnqueued;
    public event Action<int>? OnLinksChecked;

    public ModularCrawler(ILinkProcessor<T> linkProcessor)
    {
        _linkProcessor = linkProcessor;
    }

    public async Task CrawlWebsiteAsync(T startPage)
    {
        LinksChecked = 0;
        LinksEnqueued = 0;
        _visitedSites = new(DefaultQueueSize, StringComparer.OrdinalIgnoreCase);
        _enqueuedSites = new(DefaultQueueSize, StringComparer.OrdinalIgnoreCase);
        Queue<T> linkQueue = new(DefaultQueueSize);
        
        linkQueue.Enqueue(startPage);
        _enqueuedSites.Add(startPage.Target);

        while (linkQueue.TryDequeue(out var link))
        {
      
            IEnumerable<T> foundLinks = await _linkProcessor.ProcessLinkAsync(link).ConfigureAwait(false);
                
            _visitedSites.Add(link.Target);
                
            foreach (T foundLink in foundLinks)
            {
                if (!_visitedSites.Contains(foundLink.Target) && !_enqueuedSites.Contains(foundLink.Target))
                {
                    linkQueue.Enqueue(foundLink);
                    _enqueuedSites.Add(foundLink.Target);
                }
            }

            IncrementLinksChecked();
            SetResourceVisited(link);
            SetLinksEnqueued(linkQueue.Count);
        }
    }
    
    private void IncrementLinksChecked()
    {
        LinksChecked++;
        OnLinksChecked?.Invoke(LinksChecked);
    }

    private void SetLinksEnqueued(int count)
    {
        LinksEnqueued = count;
        OnLinksEnqueued?.Invoke(LinksEnqueued);
    }
    
    public void SetResourceVisited(T resource)
    {
        OnResourceVisited?.Invoke(resource);
    }
}