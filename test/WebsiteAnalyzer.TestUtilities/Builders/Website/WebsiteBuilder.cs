using System;
using System.Collections.Generic;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Utilities;

namespace WebsiteAnalyzer.TestUtilities.Builders.Website;

public class WebsiteBuilder : EntityBuilder<Core.Domain.Website.Website>
{
    public WebsiteBuilder(
        IWebsiteRepository websiteRepository,
        Guid userId,
        string url) : base(websiteRepository)
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

    public WebsiteBuilder WithBrokenLinkCrawls(ICollection<BrokenLinkCrawl> brokenLinkCrawls)
    {
        Entity.BrokenLinkCrawls = brokenLinkCrawls;
        return this;
    }
}