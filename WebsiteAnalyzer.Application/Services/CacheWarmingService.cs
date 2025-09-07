using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.models.Result;
using WebsiteAnalyzer.Core.Contracts.CacheWarm;
using WebsiteAnalyzer.Core.Contracts.Crawl;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class CacheWarmingService : ICacheWarmingService
{
    private readonly ICacheWarmRepository _cacheWarmRepository;
    private readonly ModularCrawler<Link> _linkCrawler;
    
    public CacheWarmingService(
        ICacheWarmRepository cacheWarmRepository,
        HttpClient httpClient
    )
    {
        _cacheWarmRepository = cacheWarmRepository;
        _linkCrawler = new ModularCrawler<Link>(new LinkProcessor(httpClient));
    }

    public async Task<AnonymousCacheWarm> WarmCacheAnonymous(
        string url, 
        IProgress<CrawlProgress<Link>>? progress = null, 
        CancellationToken cancellationToken = default
        )
    {
        CrawlTimer timer = new CrawlTimer();
        int linksChecked = await CrawlWebsiteCore(url, progress, cancellationToken);
        CrawlTimerResult time = timer.Complete();
        
        return new AnonymousCacheWarm(linksChecked, time.StartTime, time.EndTime);
    }

    public async Task WarmCache(
        Website website,
        CancellationToken cancellationToken = default
    )
    {
        await WarmCache(website, null, cancellationToken);
    }

    public async Task WarmCache(
        Website website, 
        IProgress<CrawlProgress<Link>>? progress = null, 
        CancellationToken cancellationToken = default
        )
    {
        CrawlTimer timer = new CrawlTimer();
        int linksChecked = await CrawlWebsiteCore(website.Url, progress, cancellationToken);
        CrawlTimerResult time = timer.Complete();
        CacheWarm cacheWarm = new CacheWarm(website, linksChecked, time.StartTime, time.EndTime);

        await _cacheWarmRepository.AddAsync(cacheWarm);
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsByWebsiteId(Guid websiteId)
    {
        return await _cacheWarmRepository.GetByWebsiteId(websiteId);
    }
    
    private async Task<int> CrawlWebsiteCore(
        string url, 
        IProgress<CrawlProgress<Link>>? progress, 
        CancellationToken cancellationToken)
    {
        int linksChecked = 0;
       
        await foreach (CrawlProgress<Link> crawlProgress in _linkCrawler.CrawlWebsiteAsync(new Link(url), cancellationToken))
        {
            linksChecked = crawlProgress.LinksChecked;
            progress?.Report(crawlProgress);
        }
       
        return linksChecked;
    }
}