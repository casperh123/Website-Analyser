using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Builders;

namespace WebsiteAnalyzer.TestUtilities.Scenarios;

public class EmailSubscriptionScenarios
{
    private readonly IEmailSubscriptionRepository _repository;

    public EmailSubscriptionScenarios(ApplicationDbContext dbContext)
    {
        _repository = new EmailSubscriptionRepository(dbContext);
    }

    public async Task<EmailSubscription> Default()
    {
        return await new EmailSubscriptionBuilder(_repository)
            .BuildAndSave();
    }

    public async Task<ICollection<EmailSubscription>> GetMultipleWithWebsiteId(int amount, Guid websiteId)
    {
        ICollection<EmailSubscription> subscriptions = [];

        for (int i = 0; i < amount; i++)
        {
            EmailSubscription subscription =
                await new EmailSubscriptionBuilder(_repository)
                    .WithWebsiteId(websiteId)
                    .BuildAndSave();
            
            subscriptions.Add(subscription);
        }

        return subscriptions;
    }
}