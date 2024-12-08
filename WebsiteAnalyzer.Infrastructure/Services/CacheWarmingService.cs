using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.DocumentParsing.ModularLinkExtraction;
using BrokenLinkChecker.models.Links;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Infrastructure.Services;

public interface ICacheWarmingService
{
    Task<CacheWarm> WarmCacheAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked);
    Task<ICollection<CacheWarm>> GetCacheWarmsAsync();
}

public class CacheWarmingService : ICacheWarmingService
{
    private readonly IWebsiteService _websiteService;
    private readonly ICacheWarmRepository _cacheWarmRepository;
    private readonly ModularCrawler<Link> _linkCrawler;

    public CacheWarmingService(
        ICacheWarmRepository cacheWarmRepository,
        IWebsiteService websiteService,
        HttpClient httpClient
        )
    {
        _cacheWarmRepository = cacheWarmRepository;
        _websiteService = websiteService; 
        
        ILinkProcessor<Link> linkProcessor = new LinkProcessor(httpClient);

        _linkCrawler = new ModularCrawler<Link>(linkProcessor);
    }

    public async Task<CacheWarm> WarmCacheAsync(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked)
    {
        Website website = await _websiteService.GetOrAddWebsite(url).ConfigureAwait(false);
        CacheWarm cacheWarm = await CreateCacheWarmEntry(website).ConfigureAwait(false);
        
        int finalLinksChecked = 0;
        
        void OnLinksChecked(int count)
        {
            finalLinksChecked = count;
            onLinkChecked(count);
        }

        try
        {
            _linkCrawler.OnLinksChecked += OnLinksChecked;
            _linkCrawler.OnLinksEnqueued += onLinkEnqueued;
            
            await _linkCrawler.CrawlWebsiteAsync(new Link(url));
        }
        finally 
        {
            _linkCrawler.OnLinksChecked -= OnLinksChecked;
            _linkCrawler.OnLinksEnqueued-= onLinkEnqueued;
        }
        
        await UpdateCacheWarmResults(cacheWarm, finalLinksChecked);
        return cacheWarm;
    }

    private async Task<CacheWarm> CreateCacheWarmEntry(Website website)
    {
        CacheWarm cacheWarm = new CacheWarm
        {
            Id = Guid.NewGuid(),
            StartTime = DateTime.Now,
            Website = website,
            WebsiteUrl = website.Url,
        };

        await _cacheWarmRepository.AddAsync(cacheWarm).ConfigureAwait(false);
        return cacheWarm;
    }

    private async Task UpdateCacheWarmResults(CacheWarm cacheWarm, int linksChecked)
    {
        cacheWarm.EndTime = DateTime.Now;
        cacheWarm.VisitedPages = linksChecked;
        await _cacheWarmRepository.UpdateAsync(cacheWarm).ConfigureAwait(false);
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsAsync()
    {
        return await _cacheWarmRepository.GetAllAsync().ConfigureAwait(false);
    }
}