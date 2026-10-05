namespace WebsiteAnalyser.Api.DTOs.Website;

public record CreateWebsiteRequest {
    public required string Url { get; set; }
    public required string Name { get; set; }
}
