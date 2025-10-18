using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Builders;
using WebsiteAnalyzer.TestUtilities.Builders.Website;

namespace WebsiteAnalyzer.TestUtilities.Scenarios;

public class ScheduledActionScenarios
{
    private readonly IScheduledActionRepository _scheduledActionRepository;
    private readonly IWebsiteRepository _websiteRepository;

    public ScheduledActionScenarios(ApplicationDbContext dbContext)
    {
        _scheduledActionRepository = new ScheduledActionRepository(dbContext);
        _websiteRepository = new WebsiteRepository(dbContext);
    }

    public async Task<ScheduledAction> CreateDefault(Website website, CrawlAction action, Frequency frequency, TimeSpan offset = default)
    {
        return await new ScheduledActionBuilder(_scheduledActionRepository, website, frequency, action, offset).BuildAndSave();
    }

    public async Task<ICollection<ScheduledAction>> CreateActionCombinations(Website website, Frequency frequency = Frequency.SixHourly, TimeSpan offset = default)
    {
        ScheduledAction brokenLinkAction = 
            await new ScheduledActionBuilder(_scheduledActionRepository, website, frequency, CrawlAction.BrokenLink, offset)
                .BuildAndSave();
        ScheduledAction cacheWarmAction =
            await new ScheduledActionBuilder(_scheduledActionRepository, website, frequency, CrawlAction.CacheWarm, offset)
                .BuildAndSave();

        return [brokenLinkAction, cacheWarmAction];
    }

    public async Task<ScheduledAction> CreateWithStatus(Status status)
    {
        Website website = await new WebsiteBuilder(_websiteRepository).BuildAndSave();
        ScheduledAction action = await new ScheduledActionBuilder(
            _scheduledActionRepository,
            website,
            Frequency.SixHourly,
            CrawlAction.BrokenLink
            ).BuildAndSave();

        return action;
    }
}