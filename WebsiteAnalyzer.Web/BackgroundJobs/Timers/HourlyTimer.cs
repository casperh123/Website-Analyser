namespace WebsiteAnalyzer.Web.BackgroundJobs.Timers;

public class HourlyTimer : IPeriodicTimer
{
    private readonly PeriodicTimer _timer;
    private bool _firstTick = true;

    public HourlyTimer()
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