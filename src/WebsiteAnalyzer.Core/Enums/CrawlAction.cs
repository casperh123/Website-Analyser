namespace WebsiteAnalyzer.Core.Enums;

public enum CrawlAction
{
    BrokenLink,
    CacheWarm,
    Uptime,
    OrderCheck
}

public static class CrawlActionExtensions
{
    public static string ToDisplayString(this CrawlAction action) => action switch
    {
        CrawlAction.BrokenLink => "Broken Link",
        CrawlAction.CacheWarm => "Cache Warm",
        CrawlAction.Uptime => "Uptime Check",
        CrawlAction.OrderCheck => "Order Check",
        _ => ""
    };
}