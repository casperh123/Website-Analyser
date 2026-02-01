namespace WebsiteAnalyzer.Core.Domain.OrderChecks;

public record OrderCheckKeys
{
    public Guid WebsiteId { get; set; }
    public string Key { get; set; }
    public string Secret { get; set; }
    
    public OrderCheckKeys() {}

    public OrderCheckKeys(Guid websiteId, string key, string secret)
    {
        WebsiteId = websiteId;
        Key = key;
        Secret = secret;
    }
}