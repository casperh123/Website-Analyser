namespace WebsiteAnalyzer.Core.Domain.OrderChecks;

public record OrderCheck
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WebsiteId { get; set; }
    public TimeSpan TimeSinceLastOrder { get; set; }
    
    public OrderCheck() {}

    public OrderCheck(Guid websiteId, TimeSpan timeSinceLastOrder)
    {
        Id = Guid.NewGuid();
        WebsiteId = websiteId;
        TimeSinceLastOrder = timeSinceLastOrder;
    }
}