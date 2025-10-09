using Test.Builders;
using WebsiteAnalyzer.Core.Entities.BrokenLink;

namespace WebsiteAnalyzer.Core.Domain.Builders.Website;

public class WebsiteBuilder : EntityBuilder<Domain.Website.Website>
{
    public WebsiteBuilder(
        ApplicationDbContext dbContext, 
        Guid userId,
        string url) : base(dbContext)
    {
        Entity = new Domain.Website.Website(
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