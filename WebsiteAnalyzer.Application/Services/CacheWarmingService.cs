using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Application.Services;

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

    public async Task WarmCache(string url, Action<int> onLinkEnqueued, Action<int> onLinkChecked,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _linkCrawler.OnLinksChecked += onLinkChecked;
            _linkCrawler.OnLinksEnqueued += onLinkEnqueued;

            await _linkCrawler.CrawlWebsiteAsync(new Link(url), cancellationToken);
        }
        finally
        {
            _linkCrawler.OnLinksChecked -= onLinkChecked;
            _linkCrawler.OnLinksEnqueued -= onLinkEnqueued;
        }
    }

    public async Task WarmCacheWithoutMetrics(string url, Guid userId, CancellationToken cancellationToken = default)
    {
        Website website = await _websiteService.GetOrAddWebsite(url, userId).ConfigureAwait(false);
        CacheWarm cacheWarm = await CreateCacheWarmEntry(website, userId).ConfigureAwait(false);

        int linksChecked = 0;

        void OnLinksChecked(int count)
        {
            linksChecked = count;
        }

        try
        {
            _linkCrawler.OnLinksChecked += OnLinksChecked;

            await _linkCrawler.CrawlWebsiteAsync(new Link(url), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _linkCrawler.OnLinksChecked -= OnLinksChecked;

            await UpdateCacheWarmResults(cacheWarm, linksChecked).ConfigureAwait(false);
        }
    }


    public async Task<CacheWarm> WarmCacheWithSaveAsync(string url, Guid userId, Action<int> onLinkEnqueued,
        Action<int> onLinkChecked, CancellationToken cancellationToken = default)
    {
        Website website = await _websiteService.GetOrAddWebsite(url, userId).ConfigureAwait(false);
        CacheWarm cacheWarm = await CreateCacheWarmEntry(website, userId).ConfigureAwait(false);

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

            await _linkCrawler.CrawlWebsiteAsync(new Link(url), cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _linkCrawler.OnLinksChecked -= OnLinksChecked;
            _linkCrawler.OnLinksEnqueued -= onLinkEnqueued;
        }

        await UpdateCacheWarmResults(cacheWarm, finalLinksChecked);

        return cacheWarm;
    }

    private async Task<CacheWarm> CreateCacheWarmEntry(Website website, Guid userId)
    {
        CacheWarm cacheWarm = new CacheWarm
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            StartTime = DateTime.Now,
            WebsiteUrl = website.Url,
        };

        await _cacheWarmRepository.AddAsync(cacheWarm).ConfigureAwait(false);
        return cacheWarm;
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsAsync()
    {
        return await _cacheWarmRepository.GetAllAsync().ConfigureAwait(false);
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid userId)
    {
        return await _cacheWarmRepository.GetCacheWarmsByUserAsync(userId);
    }

    private async Task UpdateCacheWarmResults(CacheWarm cacheWarm, int linksChecked)
    {
        cacheWarm.EndTime = DateTime.Now;
        cacheWarm.VisitedPages = linksChecked;
        await _cacheWarmRepository.UpdateAsync(cacheWarm).ConfigureAwait(false);
    }
}