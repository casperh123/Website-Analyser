using BrokenLinkChecker.Crawler.ExtendedCrawlers;
using BrokenLinkChecker.DocumentParsing.LinkProcessors;
using BrokenLinkChecker.models.Links;
using BrokenLinkChecker.models.Result;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Entities.Website;
using WebsiteAnalyzer.Core.Events;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class CacheWarmingService : ICacheWarmingService
{
    private readonly ICacheWarmRepository _cacheWarmRepository;
    private readonly ModularCrawler<Link> _linkCrawler;

    public event EventHandler<CrawlProgressEventArgs>? ProgressUpdated;

    public CacheWarmingService(
        ICacheWarmRepository cacheWarmRepository,
        HttpClient httpClient
    )
    {
        _cacheWarmRepository = cacheWarmRepository;
        _linkCrawler = new ModularCrawler<Link>(new LinkProcessor(httpClient));
    }

    public async Task WarmCache(string url, CancellationToken cancellationToken = default)
    {
        await foreach (CrawlProgress<Link> link in _linkCrawler.CrawlWebsiteAsync(new Link(url), cancellationToken))
        {
            UpdateProgress(link);
        }
    }

    public async Task WarmCacheWithoutMetrics(Website website, CancellationToken cancellationToken = default)
    {
        CacheWarm cacheWarm = await CreateCacheWarmEntry(website);

        int linksChecked = 0;
        
        try
        {
            IAsyncEnumerable<CrawlProgress<Link>> crawlProgress = _linkCrawler.CrawlWebsiteAsync(new Link(website.Url), cancellationToken);

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

    public async Task<CacheWarm> WarmCacheWithSaveAsync(string url, Website website, CancellationToken cancellationToken = default)
    {
        CacheWarm cacheWarm = await CreateCacheWarmEntry(website);

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

    public async Task<ICollection<CacheWarm>> GetCacheWarmsByWebsiteId(Guid websiteId)
    {
        return await _cacheWarmRepository.GetByWebsiteId(websiteId);
    }


    private async Task<CacheWarm> CreateCacheWarmEntry(Website website)
    {
        CacheWarm cacheWarm = new CacheWarm(website);
    
        cacheWarm.SetStartTime();
        
        await _cacheWarmRepository.AddAsync(cacheWarm);
        
        return cacheWarm;
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid? userId)
    {
        return [];
    }

    private async Task UpdateCacheWarmResults(CacheWarm cacheWarm, int linksChecked)
    {
        cacheWarm.VisitedPages = linksChecked;
        cacheWarm.SetEndTime();
    
        await _cacheWarmRepository.UpdateAsync(cacheWarm);
    }

    private void UpdateProgress(CrawlProgress<Link> progress)
    {
        ProgressUpdated?.Invoke(
            this, 
            new CrawlProgressEventArgs(
                progress.LinksEnqueued, 
                progress.LinksChecked
            )
        );
    }
}