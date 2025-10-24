using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IEmailSubcriptionService
{
    Task AddSubscription(Guid websiteId, Guid scheduledActionId, string email);
    Task Unsubscribe(Guid websiteId, Guid scheduledActionId);
    Task<IEnumerable<EmailSubscription>> GetSubscriptionsByWebsite(Guid websiteId);
}