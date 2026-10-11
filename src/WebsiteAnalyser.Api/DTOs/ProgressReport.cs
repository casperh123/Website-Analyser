namespace WebsiteAnalyser.Api.DTOs;

public record ProgressReport {
    public required int LinksEnqueued;
    public required int LinksCrawled;
}
