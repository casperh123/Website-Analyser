using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Scenarios;

namespace WebsiteAnalyzer.TestUtilities.Testing;

public class TestBase(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    protected readonly ApplicationDbContext Context = fixture.CreateContext();

    private WebsiteScenarios? _websiteScenarios;
    private ScheduledActionScenarios? _scheduledActionScenarios;
    private DowntimePingScenarios? _downtimePingScenarios;

    protected WebsiteScenarios WebsiteScenarios => _websiteScenarios ??= new WebsiteScenarios(Context);
    protected ScheduledActionScenarios ScheduledActionScenarios => _scheduledActionScenarios ??= new ScheduledActionScenarios(Context);

    protected DowntimePingScenarios DowntimePingScenarios => _downtimePingScenarios ??= new DowntimePingScenarios(Context);
    
    protected (Guid, Guid) TwoUserIds()
    {
        return (Guid.NewGuid(), Guid.NewGuid());
    }
}