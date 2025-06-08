using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class CrawlSheduleRepository : BaseRepository<CrawlSchedule>, ICrawlScheduleRepository
{
    public CrawlSheduleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }


    public async Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAsync(Guid userId)
    {
        return await DbContext.CrawlSchedules
            .Where(cs => cs.UserId == userId)
            .ToListAsync();
    }

    public async Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAndTypeAsync(Guid userId, CrawlAction action)
    {
        return await DbContext.CrawlSchedules
            .Where(cs => cs.UserId == userId)
            .Where(cs => cs.Action == action)
            .ToListAsync();
    }

    public async Task<ICollection<CrawlSchedule>> GetByAction(CrawlAction action)
    {
        return await DbContext.CrawlSchedules
            .Where(cs => cs.Action == action)
            .ToListAsync();
    }

    public async Task<CrawlSchedule?> GetCrawlScheduleBy(string url, Guid userId, CrawlAction action)
    {
        return await DbContext.CrawlSchedules
            .Where(cs => cs.Url == url)
            .Where(cs => cs.UserId == userId)
            .Where(cs => cs.Action == action)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteByUrlAndUserId(string url, Guid userId)
    {
        await DbContext.CrawlSchedules
            .Where(w => w.Url == url)
            .Where(w => w.UserId == userId)
            .ExecuteDeleteAsync();
    }
}