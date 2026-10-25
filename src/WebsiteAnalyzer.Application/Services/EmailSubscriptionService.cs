using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Exceptions;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Email;

namespace WebsiteAnalyzer.Application.Services;

public class EmailSubscriptionService : IEmailSubscriptionService
{
    private readonly IEmailSubscriptionRepository _emailRepository;
    private readonly IScheduledActionRepository _scheduledActionRepository;

    public EmailSubscriptionService(IEmailSubscriptionRepository emailRepository, IScheduledActionRepository scheduledActionRepository)
    {
        _emailRepository = emailRepository;
        _scheduledActionRepository = scheduledActionRepository;
    }

    public async Task<EmailSubscriptionDTO> Subscribe(Guid websiteId, Guid scheduledActionId, string email)
    {
        ScheduledAction? action = await _scheduledActionRepository.GetByIdAsync(scheduledActionId) ?? throw new NotFoundException("The action could not be found");
        EmailSubscription subscription = new(websiteId, action.Action, email);

        await _emailRepository.AddAsync(subscription);

        return EmailSubscriptionDTO.From(subscription);
    }

    public async Task Unsubscribe(Guid websiteId)
    {
        EmailSubscription? subscription = await _emailRepository.GetBy(websiteId);

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
