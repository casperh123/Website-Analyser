using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.models.Result;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Events;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Persistence;

namespace WebsiteAnalyzer.Application.Services;

public class CacheWarmingService : ICacheWarmingService
{
    private readonly IWebsiteService _websiteService;
    private readonly ICacheWarmRepository _cacheWarmRepository;
    private readonly ModularCrawler<Link> _linkCrawler;

    public event EventHandler<CrawlProgressEventArgs>? ProgressUpdated;

    public CacheWarmingService(
        ICacheWarmRepository cacheWarmRepository,
        IWebsiteService websiteService,
        HttpClient httpClient
    )
    {
        _cacheWarmRepository = cacheWarmRepository;
        _websiteService = websiteService;
        _linkCrawler = new ModularCrawler<Link>(new LinkProcessor(httpClient));
    }

    public async Task WarmCache(string url, CancellationToken cancellationToken = default)
    {
        await foreach (CrawlProgress<Link> link in _linkCrawler.CrawlWebsiteAsync(new Link(url), cancellationToken))
        {
            UpdateProgress(link);
        }
    }

    public async Task WarmCacheWithoutMetrics(string url, Guid userId, CancellationToken cancellationToken = default)
    {
        Website website = await _websiteService.GetOrAddWebsite(url, userId).ConfigureAwait(false);
        CacheWarm cacheWarm = await CreateCacheWarmEntry(website, userId).ConfigureAwait(false);

        int linksChecked = 0;
        
        try
        {
            IAsyncEnumerable<CrawlProgress<Link>> crawlProgress = _linkCrawler.CrawlWebsiteAsync(new Link(url), cancellationToken);

            await foreach (CrawlProgress<Link> progress in crawlProgress)
            {
                linksChecked = progress.LinksChecked;
            }
        }
        finally
        {
            await UpdateCacheWarmResults(cacheWarm, linksChecked).ConfigureAwait(false);
        }
    }


    public async Task<CacheWarm> WarmCacheWithSaveAsync(string url, Guid userId, CancellationToken cancellationToken = default)
    {
        Website website = await _websiteService.GetOrAddWebsite(url, userId).ConfigureAwait(false);
        CacheWarm cacheWarm = await CreateCacheWarmEntry(website, userId).ConfigureAwait(false);

        int linksChecked = 0;
        
        IAsyncEnumerable<CrawlProgress<Link>> crawlProgress = _linkCrawler.CrawlWebsiteAsync(new Link(url), cancellationToken);

        await foreach (CrawlProgress<Link> progress in crawlProgress)
        {
            linksChecked = progress.LinksChecked;
            UpdateProgress(progress);
        }
        
        await UpdateCacheWarmResults(cacheWarm, linksChecked);

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

        await _cacheWarmRepository.AddAsync(cacheWarm);
        return cacheWarm;
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsAsync()
    {
        return await _cacheWarmRepository.GetAllAsync();
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

    private void UpdateProgress(CrawlProgress<Link> progress)
    {
        ProgressUpdated?.Invoke(
            this, 
            new CrawlProgressEventArgs(progress.LinksEnqueued, progress.LinksChecked)
        );
    }
}