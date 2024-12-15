using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T> where T : Link
{
    private readonly ILinkProcessor<T> _linkProcessor;
    
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

    public async Task<int> CrawlWebsiteAsync(T startPage, CancellationToken token = default)
    {
        _linkProcessor.FlushCache();
        LinksChecked = 0;
        LinksEnqueued = 0;
        Queue<T> linkQueue = new(DefaultQueueSize);
        
        linkQueue.Enqueue(startPage);

        while (linkQueue.TryDequeue(out var link) && !token.IsCancellationRequested)
        {
            IEnumerable<T> foundLinks = await _linkProcessor.ProcessLinkAsync(link).ConfigureAwait(false);
            
            foreach (T foundLink in foundLinks)
            {
                linkQueue.Enqueue(foundLink);
            }
            
            IncrementLinksChecked();
            SetResourceVisited(link);
            SetLinksEnqueued(linkQueue.Count);
        }
        
        return LinksChecked;
    }
    
    private void IncrementLinksChecked()
    {
        LinksChecked++;
        OnLinksChecked?.Invoke(LinksChecked);
    }

    private void SetLinksEnqueued(int count)
    {
        LinksEnqueued = count;

        if (LinksEnqueued % 10 == 0)
        {
            OnLinksEnqueued?.Invoke(LinksEnqueued);
        }
    }
    
    public void SetResourceVisited(T resource)
    {
        OnResourceVisited?.Invoke(resource);
    }
}