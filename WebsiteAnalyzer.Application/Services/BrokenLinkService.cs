using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Channels;
using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.Models.Links;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

public class BrokenLinkService : IBrokenLinkService
{
    private readonly HttpClient _httpClient;
    private readonly IBrokenLinkCrawlRepository _crawlRepository;
    private readonly IBrokenLinkRepository _brokenLinkRepository;

    public BrokenLinkService(
        HttpClient httpClient,
        IBrokenLinkCrawlRepository crawlRepository,
        IBrokenLinkRepository brokenLinkRepository)
    {
        _httpClient = httpClient;
        _crawlRepository = crawlRepository;
        _brokenLinkRepository = brokenLinkRepository;
    }

    public async IAsyncEnumerable<IndexedLink> FindBrokenLinks(
        string url,
        Guid? userId,
        Action<int>? onLinkEnqueued,
        Action<int>? onLinkChecked,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {

        BrokenLinkProcessor linkProcessor = new BrokenLinkProcessor(_httpClient);
        ModularCrawler<IndexedLink> crawler = new ModularCrawler<IndexedLink>(linkProcessor);
        BrokenLinkCrawl? crawl = null;

        if (userId.HasValue)
        {
            crawl = await SaveBrokenLinkCrawl(url, userId.Value);
        }
        
        IndexedLink startLink = new IndexedLink(string.Empty, url, "", 0);

        crawler.OnLinksEnqueued += onLinkEnqueued;
        crawler.OnLinksChecked += onLinkChecked;
        
        await foreach (IndexedLink link in crawler.CrawlWebsiteAsync(startLink, cancellationToken))
        {
            if (link.StatusCode == HttpStatusCode.OK)
            {
                continue;
            }

            if (userId.HasValue && crawl is not null)
            {
                await SaveBrokenLinkAsync(
                    crawl,
                    link.Target,
                    link.ReferringPage,
                    link.AnchorText,
                    link.Line,
                    link.StatusCode
                    );
            }

            yield return link;
        }
    }

    private async Task SaveBrokenLinkAsync(BrokenLinkCrawl crawl, string target, string referringPage, string anchorText, int line, HttpStatusCode statusCode)
    {
        BrokenLink brokenLink = new BrokenLink(
            crawl,
            target,
            referringPage,
            anchorText,
            line,
            statusCode
        );

        await _brokenLinkRepository.AddAsync(brokenLink);
    }
    

    private async Task<BrokenLinkCrawl> SaveBrokenLinkCrawl(string url, Guid userId)
    {
        BrokenLinkCrawl crawl = new BrokenLinkCrawl(userId, url);
        await _crawlRepository.AddAsync(crawl);
        return crawl;
    }
}