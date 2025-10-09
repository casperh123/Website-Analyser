using Test.Builders;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Utilities;

namespace WebsiteAnalyzer.TestUtilities.Builders.Website;

public class WebsiteBuilder : EntityBuilder<Core.Domain.Website.Website>
{
    public WebsiteBuilder(
        ApplicationDbContext dbContext, 
        Guid userId,
        string url) : base(dbContext)
    {
        Entity = new Core.Domain.Website.Website(
            url: url,
            userId: userId,
            name: $"{StringGenerator.Generate(7)}"
        );
    }

    public WebsiteBuilder WithName(string name)
    {
        Entity.Name = name;

        return this;
    }

    public WebsiteBuilder WithBrokenLinkCrawle(ICollection<BrokenLinkCrawl> brokenLinkCrawls)
    {
        Entity.BrokenLinkCrawls = brokenLinkCrawls;
        return this;
    }
}