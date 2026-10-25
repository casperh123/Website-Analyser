using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Email;

public record EmailSubscriptionDTO {
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public CrawlAction SubscripedAction { get; set; }
    public required string Email { get; set; }

    public static EmailSubscriptionDTO From(EmailSubscription subscription) {
        return new EmailSubscriptionDTO {
            Id = subscription.Id,
            WebsiteId = subscription.WebsiteId,
            SubscripedAction = subscription.SubscripedAction,
            Email = subscription.Email
        };
    }
}
