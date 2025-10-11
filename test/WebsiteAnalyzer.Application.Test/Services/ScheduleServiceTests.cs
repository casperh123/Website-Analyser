using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Scenarios;
using WebsiteAnalyzer.TestUtilities.Testing;

namespace WebsiteAnalyzer.Application.Test.Services;

public class ScheduleServiceTests : TestBase
{
    private readonly ScheduleService _scheduleService;
    private readonly WebsiteScenarios _websiteScenarios;
    
    public ScheduleServiceTests(DatabaseFixture fixture) : base(fixture)
    {
        _websiteScenarios = new WebsiteScenarios(Context);
        IScheduledActionRepository scheduleRepository = new ScheduledActionRepository(Context);
        _scheduleService = new ScheduleService(scheduleRepository);
    }

    [Fact]
    public async Task ScheduledTask_IsDueToExecution_Immediately()
    {
        //Arrange
        Website website = await _websiteScenarios.CreateDefault(Guid.NewGuid(), "https://testwebsite.dk");

        //Act
        ScheduledAction scheduledAction = await _scheduleService.ScheduleAction(website, CrawlAction.CacheWarm, Frequency.SixHourly);

        //Assert
        Assert.True(scheduledAction.IsDueForExecution);
    }

    [Fact]
    public async Task ScheduledTask_IsNotDueForExecution_WhenNotPassedFrequency()
    {
        //Arrange
        Website website = await _websiteScenarios.CreateDefault(Guid.NewGuid(), "https://testwebsite.dk");

        //Act
        ScheduledAction scheduledAction = await _scheduleService.ScheduleAction(website, CrawlAction.CacheWarm, Frequency.SixHourly, TimeSpan.FromHours(1));
        
        //Assert
        Assert.False(scheduledAction.IsDueForExecution);
        Assert.True(scheduledAction.NextCrawlUtc <= DateTime.UtcNow.Add(TimeSpan.FromHours(1)));
    }
}
