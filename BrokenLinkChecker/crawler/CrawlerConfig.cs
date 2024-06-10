using BrokenLinkChecker.models;

namespace BrokenLinkChecker.crawler;

public class CrawlerConfig(int concurrentRequests)
{
    public readonly SemaphoreSlim Semaphore = new(concurrentRequests);
}
