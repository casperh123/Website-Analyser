namespace WebsiteAnalyzer.Web.BackgroundJobs;

public class SixHourlyTimer : IPeriodicTimer
{
    private readonly PeriodicTimer _timer;
    private bool _firstTick = true;

    public SixHourlyTimer()
    {
        _timer = new PeriodicTimer(TimeSpan.FromHours(1));
    }

    public async ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken = default)
    {
        if (_firstTick)
        {
            _firstTick = false;
            return true;
        }

        // Wait for the next periodic tick
        return await _timer.WaitForNextTickAsync(cancellationToken);
    }

    public void Dispose()
    {
        _timer.Dispose();
    }
}