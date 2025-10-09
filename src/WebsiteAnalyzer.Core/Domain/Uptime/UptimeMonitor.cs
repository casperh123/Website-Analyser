namespace WebsiteAnalyzer.Core.Domain.Uptime;

public class UptimeMonitor
{
    public UptimeMonitor(
        string websiteUrl,
        Guid userId,
        int checkIntervalMinutes = 1
    )
    {
        WebsiteUrl = websiteUrl;
        UserId = userId;
        CheckIntervalMinutes = checkIntervalMinutes;
        LastCheck = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(CheckIntervalMinutes));
    }

    public string WebsiteUrl { get; init; }
    public Guid UserId { get; init; }
    public int CheckIntervalMinutes { get; set; }
    public DateTime LastCheck { get; set; }
}