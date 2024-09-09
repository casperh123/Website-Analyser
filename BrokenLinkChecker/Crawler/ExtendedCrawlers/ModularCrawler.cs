using BrokenLinkChecker.DocumentParsing;
using BrokenLinkChecker.models.Links;

namespace BrokenLinkChecker.Crawler.ExtendedCrawlers;

public class ModularCrawler<T> where T : NavigationLink
{
    private readonly ILinkProcessor<T> _linkProcessor;
    private ModularCrawlResult<T> CrawlResult { get; }
    private Queue<NavigationLink> LinkQueue { get; set; }

    public ModularCrawler(ModularCrawlResult<T> crawlResult, ILinkProcessor<T> linkProcessor)
    { 
        CrawlResult = crawlResult;
        _linkProcessor = linkProcessor;
    }

    public async Task CrawlWebsiteAsync(Uri url)
    {
        LinkQueue = [];
        LinkQueue.Enqueue(new(url.ToString()));

        while (!LinkQueue.TryDequeue(out NavigationLink link))
        {
            await _linkProcessor.ProcessLinkAsync(link, CrawlResult);

            CrawlResult.SetLinksEnqueued(LinkQueue.Count);
        }
    }
}