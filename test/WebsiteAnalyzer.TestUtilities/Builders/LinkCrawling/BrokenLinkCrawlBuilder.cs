using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;

namespace WebsiteAnalyzer.TestUtilities.Builders.LInkCrawling;

public class BrokenLinkCrawlBuilder : EntityBuilder<BrokenLinkCrawl>
{
    public BrokenLinkCrawlBuilder(
        IBrokenLinkCrawlRepository repository,
        Website website
        ) : base(repository)
    {
        Entity = new BrokenLinkCrawl(
            website
        );
    }
}
