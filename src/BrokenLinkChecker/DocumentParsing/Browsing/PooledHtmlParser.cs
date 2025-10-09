using AngleSharp.Html.Parser;

namespace BrokenLinkChecker.DocumentParsing.Browsing;

public class PooledHtmlParser(IHtmlParser parser, HtmlParserPool pool) : IDisposable
{
    public IHtmlParser Parser => parser;

    public void Dispose()
    {
        pool.ReturnParser(parser);
    }
}