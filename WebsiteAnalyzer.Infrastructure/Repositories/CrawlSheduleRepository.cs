using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Entities;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Persistence;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class CrawlSheduleRepository : BaseRepository<CrawlSchedule>, ICrawlScheduleRepository
{
    public CrawlSheduleRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }


    public async Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAsync(Guid userId)
    {
        return await _dbContext.CrawlSchedules
            .Where(cs => cs.UserId == userId)
            .ToListAsync();
    }

    public async Task<ICollection<CrawlSchedule>> GetCrawlSchedulesByUserIdAndTypeAsync(Guid userId, CrawlAction action)
    {
        return await _dbContext.CrawlSchedules
            .Where(cs => cs.UserId == userId)
            .Where(cs => cs.Action == action)
            .ToListAsync();
    }
}