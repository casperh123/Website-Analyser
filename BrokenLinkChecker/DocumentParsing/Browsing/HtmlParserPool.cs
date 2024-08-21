using System.Collections.Concurrent;
using AngleSharp.Html.Parser;

namespace BrokenLinkChecker.DocumentParsing.Browsing
{
    public class HtmlParserPool
    {
        private readonly ConcurrentBag<IHtmlParser> _parserPool;
        private readonly SemaphoreSlim _semaphore;

        public HtmlParserPool(HtmlParserOptions config, int maxPoolSize = 1)
        {
            _parserPool = [];
            _semaphore = new SemaphoreSlim(maxPoolSize);
            
            for (int i = 0; i < maxPoolSize; i++)
            {
                HtmlParser parser = new HtmlParser(config);
                _parserPool.Add(parser);
            }
        }

        public async Task<PooledHtmlParser> GetParserAsync()
        {
            await _semaphore.WaitAsync();

            _parserPool.TryTake(out IHtmlParser parser);
            return new PooledHtmlParser(parser, this);
        }

        internal void ReturnParser(IHtmlParser parser)
        {
            _parserPool.Add(parser);
            _semaphore.Release();
        }
    }
}