using WebsiteAnalyzer.Infrastructure.Data;
using WebsiteAnalyzer.TestUtilities.Database;

namespace WebsiteAnalyzer.TestUtilities.Testing;

public class TestBase : IClassFixture<DatabaseFixture>
{
    protected ApplicationDbContext Context;

    public TestBase(DatabaseFixture fixture)
    {
        Context = fixture.CreateContext();
    }
}