namespace WebsiteAnalyser.Api.DTOs.Notifications;

public record CreateEmailSubscription {
    public Guid WebsiteId { get; set; }
    public Guid ScheduledActionId { get; set; }
    public required string Email { get; set; }
}
