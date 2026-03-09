using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IEmailSubscriptionRepository : IBaseRepository<EmailSubscription>
{
    Task<EmailSubscription?> GetBy(Guid websiteId, Guid actionId, string email);
    Task<ICollection<EmailSubscription>> GetByWebsiteId(Guid websiteId);
    Task<ICollection<EmailSubscription>> GetByWebsiteAndActionId(Guid websiteId, Guid actionId);
}