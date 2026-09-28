namespace WebsiteAnalyzer.Core.Interfaces.Services.DTOs.CacheWarm;

public class AnonymousCacheWarm
{
    public AnonymousCacheWarm(int visitedPages, DateTime startTimeUtc, DateTime endTimeUtc)
    {
        VisitedPages = visitedPages;
        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;
    }

    public int VisitedPages { get; init; }

    private DateTime StartTimeUtc { get; init; }
    private DateTime EndTimeUtc { get; init; }


    public TimeSpan TotalTime => EndTimeUtc - StartTimeUtc;
    public int AveragePageTimeMs => (int)(VisitedPages > 0 ? TotalTime.TotalMilliseconds / VisitedPages : 0);
}