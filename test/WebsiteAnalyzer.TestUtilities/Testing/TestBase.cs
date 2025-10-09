using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Scenarios;

namespace WebsiteAnalyzer.TestUtilities.Testing;

public class TestBase(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    protected readonly ApplicationDbContext Context = fixture.CreateContext();

    private WebsiteScenarios? _websiteScenarios;

    protected WebsiteScenarios WebsiteScenarios => _websiteScenarios ??= new WebsiteScenarios(Context);
}