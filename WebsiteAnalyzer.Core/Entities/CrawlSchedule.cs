using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Entities;

public record CrawlSchedule
{
    public Guid UserId { get; set; }
    public string Url { get; set; }
    public DateTime LastCrawlDate { get; set; }
    public Frequency Frequency { get; set; }
    public CrawlAction Action { get; set; }
    public Status Status { get; set; }

    public bool IsDue()
    {
        if (Status is Status.InProgress)
        {
            return false;
        }

        DateTime currentDate = DateTime.UtcNow;

        return Frequency switch
        {
            Frequency.SixHourly => LastCrawlDate.AddHours(6) <= currentDate,
            Frequency.TwelveHourly => LastCrawlDate.AddHours(12) <= currentDate,
            Frequency.Daily => LastCrawlDate.AddDays(1) <= currentDate,
            Frequency.Weekly => LastCrawlDate.AddDays(7) <= currentDate,
            _ => false
        };
    }
}