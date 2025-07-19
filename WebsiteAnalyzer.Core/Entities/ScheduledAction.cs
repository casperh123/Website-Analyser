using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Entities;

public record ScheduledAction
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public Website.Website Website { get; set; }
    public DateTime LastCrawlDate { get; set; }
    public Frequency Frequency { get; set; }
    public CrawlAction Action { get; set; }
    public Status Status { get; set; }
    
    public ScheduledAction() {}

    public ScheduledAction(
        Website.Website website,
        Frequency frequency,
        CrawlAction action
    )
    {
        Id = Guid.NewGuid();
        WebsiteId = website.Id;
        Website = website;
        Frequency = frequency;
        Action = action;
        LastCrawlDate = DateTime.Today.Subtract(TimeSpan.FromDays(14));
        Status = Status.Scheduled;
    }

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

    public void StartAction()
    {
        Status = Status.InProgress;
        LastCrawlDate = DateTime.UtcNow;
    }
}