using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Testing;

namespace WebsiteAnalyzer.Infrastructure.Test.Repositories;

public class EmailSubscriptionRepositoryTests : TestBase
{
    private readonly IEmailSubscriptionRepository _sut;
    
    public EmailSubscriptionRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
        _sut = new EmailSubscriptionRepository(DbContext);
    }

    [Fact]
    public async Task GetSubscriptionsByWebsite_GetsWebsite()
    {
        EmailSubscription subscription = await EmailSubscriptionScenarios.Default();
    }
}