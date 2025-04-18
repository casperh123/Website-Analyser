using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class CacheWarmRepository : BaseRepository<CacheWarm>, ICacheWarmRepository
{
    public CacheWarmRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public new async Task<ICollection<CacheWarm>> GetAllAsync()
    {
        return await DbContext.CacheWarms
            .OrderByDescending(cw => cw.StartTime)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid id)
    {
        return await DbContext.CacheWarms
            .Where(cw => cw.UserId == id)
            .OrderByDescending(cw => cw.StartTime)
            .ToListAsync();
    }
}