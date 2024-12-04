namespace WebsiteAnalyzer.Core.Entities;

public record CacheWarm
{
    public Guid Id { get; init; }
    public int VisitedPages { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string? WebsiteUrl { get; set; }
    public Website? Website { get; set; }
    
    public bool IsCompleted => EndTime != DateTime.MinValue;
}