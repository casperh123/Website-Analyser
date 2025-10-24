namespace WebsiteAnalyzer.Core.Domain;

public record EmailSubscription
{
    public Guid WebsiteId { get; init; }
    public Guid ScheduleActionId { get; init; }
    public string Email { get; init; }

    public EmailSubscription(Guid websiteId, Guid scheduleActionId, string email)
    {
        WebsiteId = websiteId;
        ScheduleActionId = scheduleActionId;
        Email = email;
    }
}