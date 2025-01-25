using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.Core.Persistence;

public interface ICacheWarmRepository : IBaseRepository<CacheWarm>
{
    Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid id);
}