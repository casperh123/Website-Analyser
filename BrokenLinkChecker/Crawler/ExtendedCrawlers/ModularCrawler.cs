using System.Runtime.CompilerServices;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T>(ILinkProcessor<T> linkProcessor)
    where T : Link
{
    private const int DefaultQueueSize = 1000;
    
    public Action<int>? OnLinksChecked { get; set; }
    public Action<int>? OnLinksEnqueued { get; set; }
    
    public async IAsyncEnumerable<T> CrawlWebsiteAsync(T startPage, [EnumeratorCancellation] CancellationToken token = default)
    {
        linkProcessor.FlushCache();
        
        int linksChecked = 0;
        Queue<T> linkQueue = new(DefaultQueueSize);

        linkQueue.Enqueue(startPage);

        while (linkQueue.TryDequeue(out var link) && !token.IsCancellationRequested)
        {
            IEnumerable<T> foundLinks = await linkProcessor.ProcessLinkAsync(link).ConfigureAwait(false);

            foreach (T foundLink in foundLinks)
            {
                linkQueue.Enqueue(foundLink);
            }

            linksChecked++;
            
            OnLinksChecked?.Invoke(linksChecked);
            OnLinksEnqueued?.Invoke(linkQueue.Count);
            
            yield return link;
        }
    }
}