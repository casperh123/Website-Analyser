using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Persistence;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class CacheWarmRunRepository : BaseRepository<CacheWarmRun>, ICacheWarmRunRepository
{
    public CacheWarmRunRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}