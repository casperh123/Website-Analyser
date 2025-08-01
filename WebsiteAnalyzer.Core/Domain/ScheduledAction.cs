using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Domain;

public record ScheduledAction
{
    public Guid Id { get; set; }
    public Guid WebsiteId { get; set; }
    public Entities.Website.Website Website { get; set; }
    public Frequency Frequency { get; set; }
    public CrawlAction Action { get; set; }
    
    public DateTime LastCrawlDateUtc { get; set; }
    public Status Status { get; set; }
    
    public DateTime NextCrawlUtc => LastCrawlDateUtc.Add(FrequencyExtensions.ToTimeSpan(Frequency));
    public DateTime NextCrawlLocal => NextCrawlUtc.ToLocalTime();
    public DateTime LastCrawlLocal => LastCrawlDateUtc.ToLocalTime();
    
    public bool IsDueForExecution => Status != Status.InProgress && DateTime.UtcNow >= NextCrawlUtc;
    
    private ScheduledAction() {}

    public ScheduledAction(
        Entities.Website.Website website,
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
        LastCrawlDateUtc = DateTime.UtcNow;
        Status = Status.Scheduled;
    }

    public void StartAction()
    {
        Status = Status.InProgress;
        LastCrawlDateUtc = DateTime.UtcNow;
    }
}