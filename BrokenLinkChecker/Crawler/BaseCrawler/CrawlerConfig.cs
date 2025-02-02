using BrokenLinkChecker.Crawler.BaseCrawler;

namespace BrokenLinkChecker.crawler;

public class CrawlerConfig(int concurrentRequests, CrawlMode crawlMode, bool jitter = true)
{
    public readonly int ConcurrentRequests = concurrentRequests;
    public readonly CrawlMode CrawlMode = crawlMode;
    public readonly SemaphoreSlim Semaphore = new(concurrentRequests);
    private bool Jitter => ConcurrentRequests > 1 && jitter;
    private int JitterFrequency => ConcurrentRequests * 100;

    public async Task ApplyJitterAsync()
    {
        if (!Jitter) return;

        var delay = Random.Shared.Next(0, JitterFrequency);
        await Task.Delay(delay);
    }
}