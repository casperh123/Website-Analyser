using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.TestUtilities.Builders;

public class EmailSubscriptionBuilder : EntityBuilder<EmailSubscription>
{
    public EmailSubscriptionBuilder(IEmailSubcriptionRepository repository) : base(repository)
    {
        Entity = new EmailSubscription(Guid.NewGuid(), Guid.NewGuid(), "");
    }

    public EmailSubscriptionBuilder WithWebsiteId(Guid websiteId)
    {
        Entity.WebsiteId = websiteId;

        return this;
    }

    public EmailSubscriptionBuilder WithScheduledActionId(Guid actionId)
    {
        Entity.ScheduleActionId = actionId;

        return this;
    }

    public EmailSubscriptionBuilder WithEmail(string email)
    {
        Entity.Email = email;

        return this;
    }
}