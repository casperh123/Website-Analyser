namespace WebsiteAnalyzer.Web.Models;

public record AddSubscription(Guid websiteId, Guid scheduledActionId, string email);