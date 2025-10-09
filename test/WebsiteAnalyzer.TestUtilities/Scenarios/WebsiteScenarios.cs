using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Builders.Website;

namespace WebsiteAnalyzer.TestUtilities.Scenarios;

public class WebsiteScenarios
{
    private ApplicationDbContext _dbContext;

    public WebsiteScenarios(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Website> DefaultWebsite(Guid userId, string websiteUrl)
    {
        return await new WebsiteBuilder(_dbContext, userId, websiteUrl)
            .BuildAndSave();
    }
}