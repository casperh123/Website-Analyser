using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Scenarios;

namespace WebsiteAnalyzer.TestUtilities.Testing;

public class TestBase(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    protected readonly ApplicationDbContext DbContext = fixture.CreateContext();

    private WebsiteScenarios? _websiteScenarios;
    private ScheduledActionScenarios? _scheduledActionScenarios;
    private DowntimePingScenarios? _downtimePingScenarios;
    private EmailSubscriptionScenarios? _emailSubscriptionScenarios;

    protected WebsiteScenarios WebsiteScenarios => _websiteScenarios ??= new WebsiteScenarios(DbContext);
    protected ScheduledActionScenarios ScheduledActionScenarios => _scheduledActionScenarios ??= new ScheduledActionScenarios(DbContext);
    protected DowntimePingScenarios DowntimePingScenarios => _downtimePingScenarios ??= new DowntimePingScenarios(DbContext);

    protected EmailSubscriptionScenarios EmailSubscriptionScenarios =>
        _emailSubscriptionScenarios ??= new EmailSubscriptionScenarios(DbContext);
    
    protected (Guid, Guid) TwoIds()
    {
        return (Guid.NewGuid(), Guid.NewGuid());
    }
}