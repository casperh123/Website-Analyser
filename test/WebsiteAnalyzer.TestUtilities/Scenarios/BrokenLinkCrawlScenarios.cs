using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Builders.LInkCrawling;

namespace WebsiteAnalyzer.TestUtilities.Scenarios;

public class BrokenLinkCrawlScenarios
{
    private readonly IBrokenLinkCrawlRepository _crawlRepository;

    public BrokenLinkCrawlScenarios(ApplicationDbContext dbContext)
    {
        _crawlRepository = new BrokenLinkCrawlRepository(dbContext);
    }

    public async Task<BrokenLinkCrawl> CreateDefault(Website website)
    {
        return await new BrokenLinkCrawlBuilder(_crawlRepository, website)
            .BuildAndSave();
    }
}
