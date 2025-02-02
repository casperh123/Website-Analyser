namespace WebsiteAnalyzer.Web.BackgroundJobs.Timers;

public interface IPeriodicTimer : IDisposable
{
    ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default);
}