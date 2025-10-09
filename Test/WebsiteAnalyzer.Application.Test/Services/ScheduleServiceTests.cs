using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Builders.Website;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Database;

namespace WebsiteAnalyzer.Application.Test.Services;

public class ScheduleServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly ScheduleService _scheduleService;
    private readonly ApplicationDbContext _dbContext;
    
    public ScheduleServiceTests(DatabaseFixture fixture)
    {
        _dbContext = fixture.CreateContext();
        IScheduledActionRepository scheduleRepository = new ScheduledActionRepository(_dbContext);
        _scheduleService = new ScheduleService(scheduleRepository);
    }

    [Fact]
    public async Task ScheduledTask_IsDueToExecution_Immediately()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        Website website = await new WebsiteBuilder(
            _dbContext,
            userId,
            "https://testwebsite.dk"
        ).BuildAndSave();

        //Act
        ScheduledAction scheduledAction = await _scheduleService.ScheduleAction(website, CrawlAction.CacheWarm, Frequency.SixHourly);

        //Assert
        Assert.True(scheduledAction.IsDueForExecution);
    }

    [Fact]
    public async Task ScheduledTask_IsNotDueForExecution_WhenNotPassedFrequency()
    {
        Guid userId = Guid.NewGuid();
        Website website = await new WebsiteBuilder(
            _dbContext,
            userId,
            "https://testwebsite.dk"
        ).BuildAndSave();

        //Act
        ScheduledAction scheduledAction = await _scheduleService.ScheduleAction(website, CrawlAction.CacheWarm, Frequency.SixHourly, TimeSpan.FromHours(1));
        
        //Assert
        Assert.False(scheduledAction.IsDueForExecution);
        Assert.True(scheduledAction.NextCrawlUtc <= DateTime.UtcNow.Add(TimeSpan.FromHours(1)));
    }
}
