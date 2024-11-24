using BrokenLinkChecker.crawler;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T> where T : Link
{
    private readonly ILinkProcessor<T> _linkProcessor;

    public ModularCrawler(ILinkProcessor<T> linkProcessor)
    { 
        _linkProcessor = linkProcessor;
    }

    public async Task CrawlWebsiteAsync(T startPage, ModularCrawlResult<T> crawlResult)
    {
        Queue<T> linkQueue = [];
        linkQueue.Enqueue(startPage);

        while (linkQueue.TryDequeue(out T link))
        {
            IEnumerable<T> foundLinks = await _linkProcessor.ProcessLinkAsync(link, crawlResult);

            foreach (T foundLink in foundLinks)
            {
                linkQueue.Enqueue(foundLink);
            }

            crawlResult.SetLinksEnqueued(linkQueue.Count);
        }
    }
}