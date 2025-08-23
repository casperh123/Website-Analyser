using System.Diagnostics;

namespace WebsiteAnalyzer.Core.Contracts.Crawl;

public class CrawlTimer
{
    private readonly Stopwatch _stopWatch;
    private readonly DateTime _startTime;

    public CrawlTimer()
    {
        _startTime = DateTime.UtcNow;
        _stopWatch = Stopwatch.StartNew();
    }

    public CrawlTimerResult Complete()
    {
        _stopWatch.Stop();
        DateTime endTime = _startTime.Add(_stopWatch.Elapsed);
        return new CrawlTimerResult(_startTime, endTime, _stopWatch.Elapsed);
    }
}

public class CrawlTimerResult
{
    public DateTime StartTime { get; }
    public DateTime EndTime { get; }
    public TimeSpan Duration { get; }

    public CrawlTimerResult(DateTime startTime, DateTime endTime, TimeSpan duration)
    {
        StartTime = startTime;
        EndTime = endTime;
        Duration = duration;
    }
}