using BrokenLinkChecker.models;

namespace BrokenLinkChecker.crawler;

public class CrawlerConfig(int concurrentRequests, bool jitter = true)
{
    public readonly int ConcurrentRequests = concurrentRequests;
    public readonly SemaphoreSlim Semaphore = new(concurrentRequests);
    public bool Jitter => ConcurrentRequests == 1 || jitter;
    public int JitterFrequency => ConcurrentRequests * 100;
    private readonly Random _random = new Random();

    
    public async Task ApplyJitterAsync()
    {
        if (Jitter)
        {
            int delay = _random.Next(0, JitterFrequency);
            await Task.Delay(delay);
        }
    }
}

