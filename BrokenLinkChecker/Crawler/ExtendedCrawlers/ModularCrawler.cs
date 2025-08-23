using System.Runtime.CompilerServices;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.models.Result;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T>(ILinkProcessor<T> linkProcessor)
    where T : Link
{
    private const int DefaultQueueSize = 1000;
    
    public async IAsyncEnumerable<CrawlProgress<T>> CrawlWebsiteAsync(T startPage, [EnumeratorCancellation] CancellationToken token = default)
    {
        linkProcessor.FlushCache();
        
        int linksChecked = 0;
        Queue<T> linkQueue = new(DefaultQueueSize);

        linkQueue.Enqueue(startPage);

        while (linkQueue.TryDequeue(out T? link) && !token.IsCancellationRequested)
        {
            IEnumerable<T> foundLinks = await linkProcessor.ProcessLinkAsync(link).ConfigureAwait(false);

            foreach (T foundLink in foundLinks)
            {
                linkQueue.Enqueue(foundLink);
            }

            linksChecked++;
            
            yield return new CrawlProgress<T>(link, linksChecked, linkQueue.Count);
        }
    }
}