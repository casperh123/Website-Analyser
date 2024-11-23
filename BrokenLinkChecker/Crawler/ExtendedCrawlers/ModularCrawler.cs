using BrokenLinkChecker.DocumentParsing;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T> where T : Link
{
    private readonly ILinkProcessor<T> _linkProcessor;
    private ModularCrawlResult<T> CrawlResult { get; }
    private Queue<T> LinkQueue { get; set; }

    public ModularCrawler(ModularCrawlResult<T> crawlResult, ILinkProcessor<T> linkProcessor)
    { 
        CrawlResult = crawlResult;
        _linkProcessor = linkProcessor;
    }

    public async Task CrawlWebsiteAsync(T startPage)
    {
        LinkQueue = [];
        LinkQueue.Enqueue(startPage);

        while (LinkQueue.TryDequeue(out T link))
        {
            IEnumerable<T> foundLinks = await _linkProcessor.ProcessLinkAsync(link, CrawlResult);

            foreach (T foundLink in foundLinks)
            {
                LinkQueue.Enqueue(foundLink);
            }

            CrawlResult.SetLinksEnqueued(LinkQueue.Count);
        }
    }
}