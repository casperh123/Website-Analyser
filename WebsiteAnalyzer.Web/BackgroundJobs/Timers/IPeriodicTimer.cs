namespace WebsiteAnalyzer.Web.BackgroundJobs;

public interface IPeriodicTimer : IDisposable
{
    ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default);
}