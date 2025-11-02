using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IEmailSubcriptionRepository : IBaseRepository<EmailSubscription>
{
    Task<IEnumerable<EmailSubscription>> GetSubscriptionsByWebsite(Guid websiteId);
}