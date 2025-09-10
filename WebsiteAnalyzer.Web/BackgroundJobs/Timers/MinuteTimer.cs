namespace WebsiteAnalyzer.Web.BackgroundJobs.Timers;

public class MinuteTimer : IPeriodicTimer
{
    private readonly PeriodicTimer _timer;
    private bool _firstTick = true;

    public MinuteTimer(int minutes)
    {
        _timer = new PeriodicTimer(TimeSpan.FromMinutes(minutes));
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
