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

    public async Task<Website> CreateDefault(Guid userId, string websiteUrl)
    {
        return await new WebsiteBuilder(_websiteRepository)
            .WithUserId(userId)
            .WithUrl(websiteUrl)
            .BuildAndSave();
    }

    public async Task<ICollection<Website>> CreateMultipleForUser(Guid userId, int count)
    {
        ICollection<Website> websites = [];

        for (int i = 0; i < count; i++)
        {
            websites.Add(await CreateDefault(userId, $"http://{userId}{i}.dk"));
        }

        return websites;
    }
}