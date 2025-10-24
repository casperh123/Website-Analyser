using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Repositories;

public interface IEmailSubcriptionRepository
{
    Task AddSubscription(Guid websiteId, Guid scheduledActionId, string email);
    Task Unsubscribe(Guid websiteId, Guid scheduledActionId);
    Task<IEnumerable<EmailSubscription>> GetSubscriptionsByWebsite(Guid websiteId);
}