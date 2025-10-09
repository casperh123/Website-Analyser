using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Database;

namespace WebsiteAnalyzer.TestUtilities.Testing;

public class TestBase(DatabaseFixture fixture) : IClassFixture<DatabaseFixture>
{
    protected readonly ApplicationDbContext Context = fixture.CreateContext();
}