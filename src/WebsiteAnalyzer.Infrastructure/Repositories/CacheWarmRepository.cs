using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain;
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
            .OrderByDescending(cw => cw.StartTimeUtc)
            .ToListAsync();
    }

    public async Task<ICollection<CacheWarm>> GetByWebsiteId(Guid id)
    {
        return await DbContext.CacheWarms
            .Where(cw => cw.WebsiteId == id)
            .OrderByDescending(cw => cw.EndTime)
            .ToListAsync();
    }

    public async Task<PagedResponse<CacheWarm>> GetByWebsiteId(Guid id, int page = 1, int perPage = 20) {
        IEnumerable<CacheWarm> items = await DbContext.CacheWarms
            .Where(cw => cw.WebsiteId == id)
            .OrderByDescending(cw => cw.EndTime)
            .Skip((page - 1) * perPage)
            .Take(perPage)
            .ToListAsync();

        int count = await DbContext.CacheWarms.CountAsync();

        return new PagedResponse<CacheWarm>(
            items,
            page,
            perPage,
            count
        );
    }

}
