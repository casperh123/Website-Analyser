using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface ILinkCrawlerService
{
    Task CrawlWebsiteAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked);
}

public class LinkCrawlerService : ILinkCrawlerService 
{
    private readonly ILinkProcessor<Link> _linkProcessor;

    public LinkCrawlerService(HttpClient httpClient)
    {
        _linkProcessor = new LinkProcessor(httpClient);
    }

    public async Task CrawlWebsiteAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked)
    {
        ModularCrawler<Link> crawler = new ModularCrawler<Link>(_linkProcessor);
        
        crawler.OnLinksChecked += onLinkChecked;
        crawler.OnLinksEnqueued += onLinkEnqueued;
        
        await crawler.CrawlWebsiteAsync(new Link(url));
    }
}