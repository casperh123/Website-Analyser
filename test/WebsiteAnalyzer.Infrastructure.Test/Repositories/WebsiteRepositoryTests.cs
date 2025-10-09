using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Testing;

namespace WebsiteAnalyzer.Infrastructure.Test.Repositories;

public class WebsiteRepositoryTests : TestBase
{
    private readonly IWebsiteRepository _sut;

    public WebsiteRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
        _sut = new WebsiteRepository(Context);
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsUserWebsites()
    {
        // Arrange
        Guid userId = Guid.NewGuid();

        string website1Url = "http://website1.dk";
        string website2Url = "http://website2.dk";
        string website3Url = "http://website3.dk";

        await WebsiteScenarios.DefaultWebsite(userId, website1Url);
        await WebsiteScenarios.DefaultWebsite(userId, website2Url);
        await WebsiteScenarios.DefaultWebsite(userId, website3Url);

        // Act
        ICollection<Website> retrievedWebsites = await _sut.GetAllByUserId(userId);

        // Assert
        Assert.Equal(3, retrievedWebsites.Count);
        Assert.All(retrievedWebsites, w => Assert.Equal(userId, w.UserId));
        Assert.Contains(retrievedWebsites, w => w.Url == website1Url);
        Assert.Contains(retrievedWebsites, w => w.Url == website2Url);
        Assert.Contains(retrievedWebsites, w => w.Url == website3Url);
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsOnlyUserWebsites()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();

        await WebsiteScenarios.DefaultWebsite(userId, "http://userwebsite.dk");
        await WebsiteScenarios.DefaultWebsite(userId, "http://userwebsite2.dk");
        await WebsiteScenarios.DefaultWebsite(otherUserId, "http://otheruserwebsite.dk");

        // Act 
        ICollection<Website> retrievedWebsites = await _sut.GetAllByUserId(userId);

        // Assert
        Assert.Equal(2, retrievedWebsites.Count);
        Assert.All(retrievedWebsites, w => Assert.Equal(userId, w.UserId));
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsEmpty_WhenUserHasNoWebsites()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        
        // Act
        ICollection<Website> retrievedWebsite = await _sut.GetAllByUserId(userId);
        
        // Assert
        Assert.Empty(retrievedWebsite);
    }
}