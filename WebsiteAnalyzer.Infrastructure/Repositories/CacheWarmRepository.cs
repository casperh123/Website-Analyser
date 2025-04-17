using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class CacheWarmRepository : BaseRepository<CacheWarm>, ICacheWarmRepository
{
    public CacheWarmRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
    {
    }

    public new async Task<ICollection<CacheWarm>> GetAllAsync()
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.CacheWarms
            .OrderByDescending(cw => cw.StartTime)
            .ToListAsync();
    }

    public async Task<ICollection<CacheWarm>> GetCacheWarmsByUserAsync(Guid id)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.CacheWarms
            .Where(cw => cw.UserId == id)
            .OrderByDescending(cw => cw.StartTime)
            .ToListAsync();
    }
}