using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Testing;

namespace WebsiteAnalyzer.Infrastructure.Test.Repositories;

public class BrokenLinkCrawLRepositoryTests : TestBase
{
    private readonly IBrokenLinkCrawlRepository _sut;

    public BrokenLinkCrawLRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
        _sut = new BrokenLinkCrawlRepository(DbContext);
    }
    
    // TODO GetByUserIdAsync, Single Crawl, Multiple Crawls, Multiple crawls and users
    // TODO GetByUrlAndUserId, S'
}