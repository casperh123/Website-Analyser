using BrokenLinkChecker.models;

namespace BrokenLinkChecker.crawler;

public class CrawlerConfig(int concurrentRequests, bool jitter = true)
{
    public readonly int ConcurrentRequests = concurrentRequests;
    public readonly SemaphoreSlim Semaphore = new(concurrentRequests);
    public bool Jitter => ConcurrentRequests == 1 || jitter;
    public int JitterFrequency => ConcurrentRequests * 100;
}

