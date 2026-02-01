using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;

namespace WebsiteAnalyzer.Application.Services;

public class EmailSubscriptionService : IEmailSubcriptionService
{
    private readonly IEmailSubscriptionRepository _emailRepository;

    public EmailSubscriptionService(IEmailSubscriptionRepository emailRepository)
    {
        _emailRepository = emailRepository;
    }

    public async Task<EmailSubscription> Subscribe(Guid websiteId, Guid scheduledActionId, string email)
    {
        EmailSubscription subscription = new EmailSubscription(websiteId, scheduledActionId, email);

        await _emailRepository.AddAsync(subscription);

        return subscription;
    }

    public async Task Unsubscribe(Guid websiteId, Guid scheduledActionId, string email)
    {
        EmailSubscription subscription = await _emailRepository.GetBy(websiteId, scheduledActionId, email);

        await _emailRepository.DeleteAsync(subscription);
    }

    public async Task<IEnumerable<EmailSubscription>> GetSubscriptionsByWebsite(Guid websiteId)
    {
        throw new NotImplementedException();
    }
}