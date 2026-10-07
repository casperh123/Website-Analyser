using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IEmailSubscriptionRepository : IBaseRepository<EmailSubscription>
{
    Task<EmailSubscription?> GetBy(Guid id);
    Task<ICollection<EmailSubscription>> GetByWebsiteId(Guid websiteId);
    Task<ICollection<EmailSubscription>> GetSubscriptionByWebsites(ICollection<Guid> websiteIds);
}
