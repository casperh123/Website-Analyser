using BrokenLinkChecker.models;

namespace BrokenLinkChecker.crawler;

public class CrawlerConfig(int concurrentRequests)
{
    public readonly HashSet<string> VisitedLinks = new();
    public readonly List<BrokenLink> BrokenLinks = new();
    public readonly SemaphoreSlim Semaphore = new(concurrentRequests);
}
