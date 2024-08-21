using System;
using AngleSharp.Html.Parser;

namespace BrokenLinkChecker.DocumentParsing.Browsing
{
    public class PooledHtmlParser : IDisposable
    {
        private readonly HtmlParserPool _pool;
        private IHtmlParser _parser;

        public PooledHtmlParser(IHtmlParser parser, HtmlParserPool pool)
        {
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _pool = pool ?? throw new ArgumentNullException(nameof(pool));
        }

        public IHtmlParser Parser => _parser;

        public void Dispose()
        {
            if (_parser != null)
            {
                _pool.ReturnParser(_parser);
                _parser = null; // Prevents multiple disposals from returning the parser multiple times
            }
        }
    }
}