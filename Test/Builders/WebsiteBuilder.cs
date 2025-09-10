using Test.Builders;
using test.Utilities;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Entities.BrokenLink;
using WebsiteAnalyzer.Infrastructure.Data;

namespace test.Builders;

public class WebsiteBuilder : EntityBuilder<Website>
{
    public WebsiteBuilder(
        ApplicationDbContext dbContext, 
        Guid userId,
        string url) : base(dbContext)
    {
        Entity = new Website(
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