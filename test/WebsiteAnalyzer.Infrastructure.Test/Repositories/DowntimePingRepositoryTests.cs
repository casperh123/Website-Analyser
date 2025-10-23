using WebsiteAnalyzer.Core.Domain.Uptime;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Testing;

namespace WebsiteAnalyzer.Infrastructure.Test.Repositories;

public class DowntimePingRepositoryTests : TestBase
{
    private readonly IDowntimePingRepository _sut;

    public DowntimePingRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
        _sut = new DowntimePingRepository(Context);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    public async Task GetByWebsiteIdAfterDate_ReturnsPingsAfterDate(int amount)
    {
        // Arrange
        Guid websiteId = Guid.NewGuid();
        ICollection<DowntimePing> pings = await DowntimePingScenarios.MultipleWithWebsiteId(amount, websiteId);
        
        // Act
        ICollection<DowntimePing> retrievedPings = 
            await _sut.GetByWebsiteIdAfterDate(websiteId, DateTime.Now.Subtract(TimeSpan.FromDays(20)));

        // Assert
        Assert.Equal(amount, retrievedPings.Count);
    }

    [Fact]
    public async Task GetByWebsiteIdBeforeDate_ReturnsNoPings()
    {
        // Arrange
        Guid websiteId = Guid.NewGuid();
        DateTime timeRecorded = DateTime.MaxValue;
        DowntimePing ping = await DowntimePingScenarios.WithWebsiteId(websiteId);
        
        // Act
        ICollection<DowntimePing> retrievedPings = await _sut.GetByWebsiteIdAfterDate(websiteId, DateTime.MaxValue);

        // Assert
        Assert.Empty(retrievedPings);
    }
}