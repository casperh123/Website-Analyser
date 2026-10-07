namespace WebsiteAnalyzer.Core.Domain;

public record EmailSubscription
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public Guid ScheduleActionId { get; set; }
    public string Email { get; set; }

    public EmailSubscription() { }

    public EmailSubscription(Guid websiteId, Guid scheduleActionId, string email)
    {
        Id = Guid.NewGuid();
        WebsiteId = websiteId;
        ScheduleActionId = scheduleActionId;
        Email = email;
    }
}
