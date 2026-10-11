using System.Threading.Channels;

namespace WebsiteAnalyser.Api.Streaming;

public class ThrottledProgress<T> : IProgress<T> {
    private readonly TimeSpan _interval;
    private readonly Channel<T> _channel;

    public ThrottledProgress(TimeSpan interval)  {
        _interval = interval;
        _channel = Channel.CreateBounded<T>(1);
    }

    public void Report(T value) {
        _channel.Writer.TryWrite(value);
    }

    public void Complete() {
        _channel.Writer.TryComplete();
    }

    public async Task RunAsync(Func<Task> action) {
        try  {
            await action();
        } finally {
            Complete();
        }
    }

    public async IAsyncEnumerable<T> ReadAllAsync(CancellationToken cancellationToken) {
        using PeriodicTimer timer = new PeriodicTimer(_interval);
        ChannelReader<T> reader = _channel.Reader;

        while(await timer.WaitForNextTickAsync()) {
            T? latest = default;

            while(reader.TryRead(out T? item)) {
                latest = item;
            }

            if(latest is not null) {
                yield return latest;
            }

            if(reader.Completion.IsCompleted) {
                yield break;
            }
        }
    }
}
