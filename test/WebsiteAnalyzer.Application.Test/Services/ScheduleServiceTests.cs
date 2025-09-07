using test.Database;
using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Enums;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.Infrastructure.Repositories;
using Xunit;

namespace test.Builders;

public class ScheduleServiceTests : IClassFixture<ApplicationDbContext>
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
    public async Task Something()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
        Website website = await new WebsiteBuilder(
            _dbContext,
            userId,
            "https://testwebsite.dk"
            ).BuildAndSave();

        //Act
        ScheduledAction scheduledAction = await _scheduleService.ScheduleAction(website, CrawlAction.CacheWarm, Frequency.SixHourly, TimeSpan.MinValue);
        
        
    }
}