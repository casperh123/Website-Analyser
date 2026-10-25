using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Domain;

public record EmailSubscription
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public CrawlAction SubscripedAction { get; set; }
    public string Email { get; set; }

    public EmailSubscription() { }

    public EmailSubscription(Guid websiteId, CrawlAction subscripedAction, string email)
    {
        Id = Guid.NewGuid();
        WebsiteId = websiteId;
        SubscripedAction = subscripedAction;
        Email = email;
    }
}
