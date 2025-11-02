using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Testing;

namespace WebsiteAnalyzer.Application.Test.Services;

public class ScheduleServiceTests : TestBase
{
    private readonly IScheduleService _sut;
    
    public ScheduleServiceTests(DatabaseFixture fixture) : base(fixture)
    {
        IScheduledActionRepository scheduleRepository = new ScheduledActionRepository(DbContext);
        _sut = new ScheduleService(scheduleRepository);
    }

    [Fact]
    public async Task ScheduledTask_IsDueToExecution_Immediately()
    {
        //Arrange
        Website website = await WebsiteScenarios.CreateDefault(Guid.NewGuid(), "https://testwebsite.dk");

        //Act
        ScheduledAction scheduledAction = await _sut.ScheduleAction(website, CrawlAction.CacheWarm, Frequency.SixHourly);

        //Assert
        Assert.True(scheduledAction.IsDueForExecution);
    }

    [Fact]
    public async Task ScheduledTask_IsNotDueForExecution_WhenNotPassedFrequency()
    {
        //Arrange
        Website website = await WebsiteScenarios.CreateDefault(Guid.NewGuid(), "https://testwebsite.dk");

        //Act
        ScheduledAction scheduledAction = await _sut.ScheduleAction(website, CrawlAction.CacheWarm, Frequency.SixHourly, TimeSpan.FromHours(1));
        
        //Assert
        Assert.False(scheduledAction.IsDueForExecution);
        Assert.True(scheduledAction.NextCrawlUtc <= DateTime.UtcNow.Add(TimeSpan.FromHours(1)));
    }

    [Fact]
    public async Task GetDueSchedulesByAction_Returns_ScheduledItems()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Website website = await WebsiteScenarios.CreateDefault(userId, "http://website.dk");
        ICollection<ScheduledAction> scheduledActions = await ScheduledActionScenarios.CreateActionCombinations(website);
        
        // Act
        ICollection<ScheduledAction> dueActions = await _sut.GetDueSchedulesBy(CrawlAction.BrokenLink);

        // Assert
        Assert.Single(dueActions);
    }

    [Fact]
    public async Task ResetActionStatus_ResetStatus()
    {
        // Arrange
        ScheduledAction scheduledAction = await ScheduledActionScenarios.CreateWithStatus(Status.InProgress);
        
        // Act
        await _sut.ResetActionStatus(scheduledAction);
        ScheduledAction retrievedAction = await _sut.GetById(scheduledAction.Id);

        // Assert
        Assert.Equal(Status.Scheduled, scheduledAction.Status);
    }
}
