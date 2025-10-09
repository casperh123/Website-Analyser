namespace WebsiteAnalyzer.Core.Contracts.CacheWarm;

public class AnonymousCacheWarm
{
    public int VisitedPages { get; init; }
    
    private DateTime StartTimeUtc { get; init; }
    private DateTime EndTimeUtc { get; init; }
    
    // UI-friendly properties
    public DateTime StartTime => StartTimeUtc.ToLocalTime();
    public DateTime EndTime => EndTimeUtc.ToLocalTime();
    
    public TimeSpan TotalTime => EndTimeUtc - StartTimeUtc;
    public int AveragePageTimeMs => (int)(VisitedPages > 0 ? TotalTime.TotalMilliseconds / VisitedPages : 0);
    
    public AnonymousCacheWarm(int visitedPages, DateTime startTimeUtc, DateTime endTimeUtc)
    {
        VisitedPages = visitedPages;
        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;
    }
}