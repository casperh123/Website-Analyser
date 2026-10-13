namespace WebsiteAnalyser.Api.DTOs;

public record ProgressReport {
    public required int LinksEnqueued { get; set; }
    public required int LinksCrawled { get; set; }
}
