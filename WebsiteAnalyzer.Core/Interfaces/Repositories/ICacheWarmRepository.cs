using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Core.Persistence;

public interface ICacheWarmRepository : IBaseRepository<CacheWarm>
{
    Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid id);
}