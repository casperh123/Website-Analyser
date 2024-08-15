using System.Collections.Concurrent;
using AngleSharp;

namespace BrokenLinkChecker.DocumentParsing;

public class BrowsingContextPool
{
    private readonly ConcurrentBag<IBrowsingContext> _contextPool;
    private readonly IConfiguration _config;
    private readonly int _maxPoolSize;

    public BrowsingContextPool(IConfiguration config, int maxPoolSize = 1)
    {
        _config = config;
        _maxPoolSize = maxPoolSize;
        _contextPool = new ConcurrentBag<IBrowsingContext>();
        
        // Pre-fill the pool with initial contexts if desired
        for (int i = 0; i < _maxPoolSize; i++)
        {
            _contextPool.Add(BrowsingContext.New(_config));
        }
    }

    public IBrowsingContext GetContext()
    {
        if (_contextPool.TryTake(out IBrowsingContext context))
        {
            return context;
        }

        // Create a new context if the pool is exhausted
        return BrowsingContext.New(_config);
    }

    public void ReturnContext(IBrowsingContext context)
    {
        if (_contextPool.Count < _maxPoolSize)
        {
            _contextPool.Add(context);
        }
    }
}