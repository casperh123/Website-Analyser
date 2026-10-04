using WebsiteAnalyzer.Core.Domain;

namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Email;

public record EmailSubscriptionDTO {
    public Guid WebsiteId { get; set; }
    public Guid ScheduledActionId { get; set; }
    public required string Email { get; set; }

    public static EmailSubscriptionDTO From(EmailSubscription subscription) {
        return new EmailSubscriptionDTO {
            WebsiteId = subscription.WebsiteId,
            ScheduledActionId = subscription.ScheduleActionId,
            Email = subscription.Email
        };
    }
}
