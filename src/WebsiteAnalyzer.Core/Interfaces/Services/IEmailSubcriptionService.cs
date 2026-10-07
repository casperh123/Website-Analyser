using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Email;

namespace WebsiteAnalyzer.Core.Interfaces.Services;

public interface IEmailSubscriptionService
{
    Task<EmailSubscriptionDTO> Subscribe(Guid websiteId, Guid scheduledActionId, string email);
    Task Unsubscribe(Guid id);
    Task<ICollection<EmailSubscriptionDTO>> GetSubscriptionsByWebsite(Guid websiteId);
    Task<ICollection<EmailSubscriptionDTO>> GetSubscriptionByWebsites(ICollection<Guid> websiteIds);
}
