using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class ScheduledActionRepository : BaseRepository<ScheduledAction>, IScheduledActionRepository
{
    public ScheduledActionRepository(ApplicationDbContext dbContext) : base(dbContext)
    {
    }

    public async Task<ScheduledAction> GetByWebsiteIdAndType(Guid websiteId, CrawlAction type)
    {
        return await DbContext.ScheduledActions
            .Where(sa => sa.WebsiteId == websiteId)
            .Where(sa => sa.Action == type)
            .FirstAsync();
    }

    public async Task<ICollection<ScheduledAction>> GetCrawlSchedulesByUserIdAndTypeAsync(Guid userId, CrawlAction action)
    {
        return await DbContext.ScheduledActions
            .Where(cs => cs.Website.UserId == userId)
            .Where(cs => cs.Action == action)
            .ToListAsync();
    }

    public async Task<ICollection<ScheduledAction>> GetByAction(CrawlAction action)
    {
        return await DbContext.ScheduledActions
            .Where(cs => cs.Action == action)
            .Include(cs => cs.Website)
            .ToListAsync();
    }

    public async Task<ScheduledAction?> GetCrawlScheduleBy(string url, Guid userId, CrawlAction action)
    {
        return await DbContext.ScheduledActions
            .Where(cs => cs.Website.Url == url)
            .Where(cs => cs.Website.UserId == userId)
            .Where(cs => cs.Action == action)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteByUrlAndUserId(string url, Guid userId)
    {
        await DbContext.ScheduledActions
            .Where(w => w.Website.Url == url)
            .Where(w => w.Website.UserId == userId)
            .ExecuteDeleteAsync();
    }

    public async Task<ScheduledAction> GetActionByWebsiteId(Guid websiteId)
    {
        return await DbContext.ScheduledActions
            .Where(action => action.WebsiteId == websiteId)
            .FirstAsync();
    }
}