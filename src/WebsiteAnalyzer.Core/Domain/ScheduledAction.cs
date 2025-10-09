using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Domain;

public record ScheduledAction
{
    private ScheduledAction()
    {
    }

    public ScheduledAction(
        Website.Website website,
        Frequency frequency,
        CrawlAction action,
        TimeSpan offset
    )
    {
        Id = Guid.NewGuid();
        WebsiteId = website.Id;
        Website = website;
        Frequency = frequency;
        Action = action;
        LastCrawlDateUtc = CalculateFirstCrawl(offset);
        Status = Status.Scheduled;
    }

    public Guid Id { get; init; }
    public Guid WebsiteId { get; init; }
    public Website.Website Website { get; init; }
    public Frequency Frequency { get; set; }
    public CrawlAction Action { get; init; }

    public DateTime LastCrawlDateUtc { get; set; }
    public Status Status { get; set; }

    public DateTime NextCrawlUtc => LastCrawlDateUtc.Add(FrequencyExtensions.ToTimeSpan(Frequency));
    public DateTime NextCrawlLocal => NextCrawlUtc.ToLocalTime();
    public DateTime LastCrawlLocal => LastCrawlDateUtc.ToLocalTime();

    public bool IsDueForExecution => Status != Status.InProgress && DateTime.UtcNow >= NextCrawlUtc;

    private DateTime CalculateFirstCrawl(TimeSpan offset)
    {
        DateTime currentTime = DateTime.UtcNow;
        DateTime firstCrawl = currentTime.Subtract(FrequencyExtensions.ToTimeSpan(Frequency));

        return firstCrawl.Add(offset);
    }
}