using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class CrawlSheduleRepository : BaseRepository<CrawlSchedule>, ICrawlScheduleRepository
{
    public CrawlSheduleRepository(IDbContextFactory<ApplicationDbContext> dbContextFactory) : base(dbContextFactory)
    {
    }


    public async Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAsync(Guid userId)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.CrawlSchedules
            .Where(cs => cs.UserId == userId)
            .ToListAsync();
    }

    public async Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAndTypeAsync(Guid userId, CrawlAction action)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.CrawlSchedules
            .Where(cs => cs.UserId == userId)
            .Where(cs => cs.Action == action)
            .ToListAsync();
    }

    public async Task<ICollection<CrawlSchedule>> GetByAction(CrawlAction action)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.CrawlSchedules
            .Where(cs => cs.Action == action)
            .ToListAsync();
    }

    public async Task<CrawlSchedule?> GetCrawlScheduleBy(string url, Guid userId, CrawlAction action)
    {
        await using ApplicationDbContext dbContext = await DbContextFactory.CreateDbContextAsync();

        return await dbContext.CrawlSchedules
            .Where(cs => cs.Url == url)
            .Where(cs => cs.UserId == userId)
            .Where(cs => cs.Action == action)
            .FirstOrDefaultAsync();
    }
}