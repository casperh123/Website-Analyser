using WebsiteAnalyzer.Core.Enums;

namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.ScheduledAction;

public record ScheduledActionDTO
{
    public Guid Id { get; init; }
    public Guid WebsiteId { get; init; }
    public Domain.Website Website { get; init; }
    public Frequency Frequency { get; set; }
    public CrawlAction Action { get; init; }
    public DateTime LastCrawlDateUtc { get; set; }
    public Status Status { get; set; }
    public DateTime NextCrawlUtc { get; set; }
    public DateTime NextCrawlLocal { get; set; }
    public DateTime LastCrawlLocal { get; set; }
    public bool IsDueForExecution { get; set; }

    public static ScheduledActionDTO From(Domain.ScheduledAction action)
    {
        return new ScheduledActionDTO
        {
            Id = action.Id,
            WebsiteId = action.WebsiteId,
            Website = action.Website,
            Frequency = action.Frequency,
            Action = action.Action,
            LastCrawlDateUtc = action.LastCrawlDateUtc,
            Status = action.Status,
            NextCrawlUtc = action.NextCrawlUtc,
            NextCrawlLocal = action.NextCrawlLocal,
            LastCrawlLocal = action.LastCrawlLocal,
            IsDueForExecution = action.IsDueForExecution
        };
    }
}