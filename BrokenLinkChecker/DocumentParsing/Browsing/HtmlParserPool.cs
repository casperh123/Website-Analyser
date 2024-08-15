using System.Collections.Concurrent;
using AngleSharp;
using AngleSharp.Html.Parser;

namespace BrokenLinkChecker.DocumentParsing.Browsing
{
    public class HtmlParserPool
    {
        private readonly ConcurrentBag<IHtmlParser> _parserPool;
        private readonly SemaphoreSlim _semaphore;

        public HtmlParserPool(HtmlParserOptions config, int maxPoolSize = 1)
        {
            _parserPool = new ConcurrentBag<IHtmlParser>();
            _semaphore = new SemaphoreSlim(maxPoolSize, maxPoolSize);

            // Pre-fill the pool with the exact number of parsers
            for (int i = 0; i < maxPoolSize; i++)
            {
                var parser = new HtmlParser(config);
                _parserPool.Add(parser);
            }
        }

        public async Task<PooledHtmlParser> GetParserAsync()
        {
            // Wait until a parser is available in the pool
            await _semaphore.WaitAsync();

            _parserPool.TryTake(out IHtmlParser parser);
            return new PooledHtmlParser(parser, this);
        }

        internal void ReturnParser(IHtmlParser parser)
        {
            _parserPool.Add(parser);
            _semaphore.Release(); // Release a slot in the semaphore to signal availability
        }
    }
}