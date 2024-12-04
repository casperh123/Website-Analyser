using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Persistence;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class CacheWarmRepository : BaseRepository<CacheWarm>, ICacheWarmRepository
{
    public CacheWarmRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }
}