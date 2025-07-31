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
    public DateTime NextCrawl => LastCrawlDate.Add(FrequencyExtensions.ToTimeSpan(Frequency));
    
    private ScheduledAction() {}

    public ScheduledAction(
        Website.Website website,
        Frequency frequency,
        CrawlAction action,
        DateTime firstCrawl
        )
    {
        Id = Guid.NewGuid();
        WebsiteId = website.Id;
        Website = website;
        Frequency = frequency;
        Action = action;
        LastCrawlDate = firstCrawl;
        Status = Status.Scheduled;
    }

    public bool IsDueForExecution()
    {
        if (Status is Status.InProgress) return false;
        
        DateTime currentTime = DateTime.Now;
        DateTime nextDueTime = CalculateNextDueTime();

        return currentTime >= nextDueTime;
    }

    private DateTime CalculateNextDueTime() => Frequency switch
    {
        Frequency.SixHourly => LastCrawlDate.AddHours(6),
        Frequency.TwelveHourly => LastCrawlDate.AddHours(12),
        Frequency.Daily => LastCrawlDate.AddDays(1),
        Frequency.Weekly => LastCrawlDate.AddDays(7),
        _ => throw new InvalidOperationException($"Unsupported frequency: {Frequency}")
    };

    public void StartAction()
    {
        Status = Status.InProgress;
        LastCrawlDate = DateTime.Now;
    }
}