using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class EmailSubscriptionRepository(ApplicationDbContext dbContext)
    : BaseRepository<EmailSubscription>(dbContext), IEmailSubscriptionRepository
{
    public async Task<EmailSubscription?> GetBy(Guid id)
    {
        return await DbContext.EmailSubcriptions
            .Where(e => e.Id == id)
            .FirstOrDefaultAsync();
    }

    public async Task<ICollection<EmailSubscription>> GetByWebsiteId(Guid websiteId)
    {
        return await DbContext.EmailSubcriptions
            .Where(sub => sub.WebsiteId == websiteId)
            .OrderBy(sub => sub.Email)
            .ToListAsync();
    }

    public async Task<ICollection<EmailSubscription>> GetSubscriptionByWebsites(ICollection<Guid> websiteIds)
    {
        return await DbContext.EmailSubcriptions
            .Where(s => websiteIds.Contains(s.WebsiteId))
            .ToListAsync();
    }

    public async Task<ICollection<EmailSubscription>> GetByWebsiteAndActionId(Guid websiteId, Guid actionId)
    {
        return await DbContext.EmailSubcriptions
            .Where(s => s.WebsiteId == websiteId)
            .Where(s => s.ScheduleActionId == actionId)
            .ToListAsync();
    }
}
