using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IEmailSubcriptionService
{
    Task<EmailSubscription> Subscribe(Guid websiteId, Guid scheduledActionId, string email);
    Task Unsubscribe(Guid websiteId, Guid scheduledActionId, string email);
    Task<IEnumerable<EmailSubscription>> GetSubscriptionsByWebsite(Guid websiteId);
}