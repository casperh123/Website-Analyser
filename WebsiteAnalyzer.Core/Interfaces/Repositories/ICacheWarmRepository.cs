using WebsiteAnalyzer.Core.Entities;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface ICacheWarmRepository : IBaseRepository<CacheWarm>
{
    Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid id);
}