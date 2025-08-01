using WebsiteAnalyzer.Core.Entities.BrokenLink;

namespace WebsiteAnalyzer.Core.Contracts;

public class BrokenLinkCrawlSession
{
    private BrokenLinkCrawlDTO BrokenLinkCrawl { get; init; }
    private IAsyncEnumerable<BrokenLinkDTO> BrokenLinks { get; init; }

    public BrokenLinkCrawlSession(BrokenLinkCrawlDTO brokenLinkCrawl, IAsyncEnumerable<BrokenLinkDTO> brokenLinks)
    {
        BrokenLinkCrawl = brokenLinkCrawl;
        BrokenLinks = brokenLinks;
    }
}