using BrokenLinkChecker.Models.Links;
using BrokenLinkChecker.Models.Result;
using WebsiteAnalyzer.Core.Contracts.CacheWarm;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface ICacheWarmingService
{
    Task<AnonymousCacheWarm> WarmCacheAnonymous(string url, 
        IProgress<CrawlProgress<Link>>? progress = null, 
        CancellationToken cancellationToken = default);
    Task WarmCache(Website website, IProgress<CrawlProgress<Link>>? progress = null, CancellationToken cancellationToken = default);
    Task WarmCache(Website website, CancellationToken cancellationToken = default);
    Task<ICollection<CacheWarm>> GetCacheWarmsByWebsiteId(Guid websiteId);
}