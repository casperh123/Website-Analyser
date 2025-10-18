using System;
using System.Collections.Generic;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Utilities;

namespace WebsiteAnalyzer.TestUtilities.Builders.Website;

public class WebsiteBuilder : EntityBuilder<Core.Domain.Website.Website>
{
    public WebsiteBuilder(IWebsiteRepository websiteRepository) : base(websiteRepository)
    {
        Entity = new Core.Domain.Website.Website(
            url: $"http://{StringGenerator.Generate(7)}.dk",
            userId: Guid.NewGuid(),
            name: $"{StringGenerator.Generate(7)}"
        );
    }

    public WebsiteBuilder WithUserId(Guid userId)
    {
        Entity.UserId = userId;

        return this;
    }

    public WebsiteBuilder WithUrl(string url)
    {
        Entity.Url = url;

        return this;
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