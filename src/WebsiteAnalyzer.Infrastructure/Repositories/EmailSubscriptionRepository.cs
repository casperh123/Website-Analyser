using Microsoft.EntityFrameworkCore;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;

namespace WebsiteAnalyzer.Infrastructure.Repositories;

public class EmailSubscriptionRepository(ApplicationDbContext dbContext)
    : BaseRepository<EmailSubscription>(dbContext), IEmailSubcriptionRepository
{

    public async Task<IEnumerable<EmailSubscription>> GetSubscriptionsByWebsite(Guid websiteId)
    {
        return await dbContext.EmailSubcriptions
            .Where(sub => sub.WebsiteId == websiteId)
            .ToListAsync();
    }
}