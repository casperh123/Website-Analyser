using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface ICacheWarmRepository : IBaseRepository<CacheWarm>
{
    Task<ICollection<CacheWarm>> GetByWebsiteId(Guid id);
    Task<PagedResponse<CacheWarm>> GetByWebsiteId(Guid id, int page = 1, int perPage = 20);
}
