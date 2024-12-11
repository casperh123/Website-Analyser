using System.Globalization;
using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Persistence;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class CacheWarmRepository : BaseRepository<CacheWarm>, ICacheWarmRepository
{
    public CacheWarmRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public new async Task<ICollection<CacheWarm>> GetAllAsync()
    {
        return await _dbContext.CacheWarms
            .Include(ch => ch.Website)
            .OrderByDescending(cw => cw.StartTime)
            .ToListAsync().ConfigureAwait(false);
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid id)
    {
        return await _dbContext.CacheWarms
            .Include(cw => cw.Website)
            .Where(cw => cw.Website.UserId == id)
            .OrderByDescending(cw => cw.StartTime)
            .ToListAsync();
    }
}