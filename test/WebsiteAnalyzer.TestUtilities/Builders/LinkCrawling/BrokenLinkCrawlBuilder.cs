using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.TestUtilities.Builders.LInkCrawling;

public class BrokenLinkCrawlBuilder : EntityBuilder<BrokenLinkCrawl>
{
    public BrokenLinkCrawlBuilder(
        IBrokenLinkCrawlRepository repository,
        Guid userId,
        string url
        ) : base(repository)
    {
        Entity = new BrokenLinkCrawl(
            userId,
            url
        );
    }
}