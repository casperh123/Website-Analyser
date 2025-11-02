namespace WebsiteAnalyzer.Core.Domain;

public record EmailSubscription
{
    public Guid WebsiteId { get; set; }
    public Guid ScheduleActionId { get; set; }
    public string Email { get; set; }

    public EmailSubscription(Guid websiteId, Guid scheduleActionId, string email)
    {
        WebsiteId = websiteId;
        ScheduleActionId = scheduleActionId;
        Email = email;
    }
}