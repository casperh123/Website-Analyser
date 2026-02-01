using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class EmailSubscriptionRepository(ApplicationDbContext dbContext)
    : BaseRepository<EmailSubscription>(dbContext), IEmailSubscriptionRepository
{
    public async Task<EmailSubscription?> GetBy(Guid websiteId, Guid actionId, string email)
    {
        return await DbContext.EmailSubcriptions
            .Where(e => e.WebsiteId == websiteId)
            .Where(e => e.ScheduleActionId == actionId)
            .Where(e => e.Email == email)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<EmailSubscription>> GetByWebsiteId(Guid websiteId)
    {
        return await DbContext.EmailSubcriptions
            .Where(sub => sub.WebsiteId == websiteId)
            .ToListAsync();
    }

    public async Task<IEnumerable<EmailSubscription>> GetByWebsiteAndActionId(Guid websiteId, Guid actionId)
    {
        return await DbContext.EmailSubcriptions
            .Where(s => s.WebsiteId == websiteId)
            .Where(s => s.ScheduleActionId == actionId)
            .ToListAsync();
    }
}