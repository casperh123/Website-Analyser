using BrokenLinkChecker.models;

namespace BrokenLinkChecker.crawler;

public class CrawlerConfig(int concurrentRequests, bool jitter = true)
{
    public readonly SemaphoreSlim Semaphore = new(concurrentRequests);
    public bool Jitter => concurrentRequests == 1 || jitter;
}

