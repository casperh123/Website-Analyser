using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Builders.Website;

namespace WebsiteAnalyzer.TestUtilities.Scenarios;

public class WebsiteScenarios
{
    private readonly IWebsiteRepository _websiteRepository;

    public WebsiteScenarios(ApplicationDbContext dbContext)
    {
        _websiteRepository = new WebsiteRepository(dbContext);
    }

    public async Task<Website> DefaultWebsite(Guid userId, string websiteUrl)
    {
        return await new WebsiteBuilder(_websiteRepository, userId, websiteUrl)
            .BuildAndSave();
    }
}