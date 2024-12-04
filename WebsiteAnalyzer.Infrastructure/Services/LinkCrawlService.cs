using BrokenLinkChecker.crawler;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface ILinkCrawlerService
{
    Task<ModularCrawlResult<Link>> CrawlWebsiteAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked);
}

public class LinkCrawlerService : ILinkCrawlerService 
{
    private readonly ILinkProcessor<Link> _linkProcessor;

    public LinkCrawlerService(HttpClient httpClient)
    {
        _linkProcessor = new LinkProcessor(httpClient);
    }

    public async Task<ModularCrawlResult<Link>> CrawlWebsiteAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked)
    {
        ModularCrawlResult<Link> crawlResult = GenerateCrawlResult(onLinkEnqueued, onLinkChecked);
        ModularCrawler<Link> crawler = new ModularCrawler<Link>(_linkProcessor);
        
        await crawler.CrawlWebsiteAsync(new Link(url), crawlResult);
        return crawlResult;
    }

    private ModularCrawlResult<Link> GenerateCrawlResult(Action<int> onLinksEnqueued, Action<int> onLinksChecked)
    {
        ModularCrawlResult<Link> result = new ModularCrawlResult<Link>();
        
        result.OnLinksEnqueued += onLinksEnqueued;
        result.OnLinksChecked += onLinksChecked;
        
        return result;
    }
}