using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IEmailSubcriptionService
{
    Task<EmailSubscription> Subscribe(Guid websiteId, Guid scheduledActionId, string email);
    Task Unsubscribe(Guid websiteId, Guid scheduledActionId, string email);
    Task<ICollection<EmailSubscription>> GetSubscriptionsByWebsite(Guid websiteId);
    Task<ICollection<EmailSubscription>> GetSubscriptionByWebsites(ICollection<Guid> websiteIds);
}