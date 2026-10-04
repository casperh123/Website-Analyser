using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Email;

namespace WebsiteAnalyzer.Application.Services;

public class EmailSubscriptionService : IEmailSubcriptionService
{
    private readonly IEmailSubscriptionRepository _emailRepository;

    public EmailSubscriptionService(IEmailSubscriptionRepository emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<EmailSubscriptionDTO> Subscribe(Guid websiteId, Guid scheduledActionId, string email)
    {
        EmailSubscription subscription = new EmailSubscription(websiteId, scheduledActionId, email);

        await _emailRepository.AddAsync(subscription);

        return EmailSubscriptionDTO.From(subscription);
    }

    public async Task Unsubscribe(Guid websiteId, Guid scheduledActionId, string email)
    {
        EmailSubscription? subscription = await _emailRepository.GetBy(websiteId, scheduledActionId, email);

        if (subscription is null)
        {
            return;
        }

        await _emailRepository.DeleteAsync(subscription);
    }

    public async Task<ICollection<EmailSubscriptionDTO>> GetSubscriptionsByWebsite(Guid websiteId)
    {
        ICollection<EmailSubscription> subscriptions = await _emailRepository.GetByWebsiteId(websiteId);

        return [.. subscriptions.Select(EmailSubscriptionDTO.From)];
    }

    public async Task<ICollection<EmailSubscriptionDTO>> GetSubscriptionByWebsites(ICollection<Guid> websiteIds)
    {
        ICollection<EmailSubscription> subscriptions = await _emailRepository.GetSubscriptionByWebsites(websiteIds);

        return [.. subscriptions.Select(EmailSubscriptionDTO.From)];
    }
}
