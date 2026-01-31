namespace WebsiteAnalyzer.Core.Domain.OrderChecks;

public record OrderCheck(Guid WebsiteId, TimeSpan TimeSinceLastOrder)
{
    public Guid Id { get; set; } = Guid.NewGuid();
}