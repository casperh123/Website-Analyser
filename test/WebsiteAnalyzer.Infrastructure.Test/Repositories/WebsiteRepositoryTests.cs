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

    [Fact]
    public async Task GetByIdAndUserId_ReturnsUserAndIdWebsite()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Website website = await WebsiteScenarios.DefaultWebsite(userId, "http://website.dk");
        Guid websiteId = website.Id;
        
        // Act
        Website? retrievedWebsite = await _sut.GetByIdAndUserId(websiteId, userId);

        Assert.NotNull(retrievedWebsite);
        Assert.Equal(userId, retrievedWebsite.UserId);
        Assert.Equal(websiteId, retrievedWebsite.Id);
    }

    [Fact]
    public async Task GetByIdAndUserId_ReturnsWebsite_WhenMultipleWebsites()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Website website = await WebsiteScenarios.DefaultWebsite(userId, "http://website.dk");
        Guid websiteId = website.Id;
        await WebsiteScenarios.DefaultWebsite(userId, "http://website2.dk");

        // Act
        Website? retrievedWebsite = await _sut.GetByIdAndUserId(websiteId, userId);
        
        // Assert
        Assert.NotNull(retrievedWebsite);
        Assert.Equal(userId, retrievedWebsite.UserId);
        Assert.Equal(websiteId, retrievedWebsite.Id);
    }
    
    [Fact]
    public async Task GetByIdAndUserId_ReturnsWebsite_WhenMultipleUsersAndWebsites()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Website website = await WebsiteScenarios.DefaultWebsite(userId, "http://website.dk");
        Guid websiteId = website.Id;
        await WebsiteScenarios.DefaultWebsite(otherUserId, "http://website2.dk");

        // Act
        Website? retrievedWebsite = await _sut.GetByIdAndUserId(websiteId, userId);
        
        // Assert
        Assert.NotNull(retrievedWebsite);
        Assert.Equal(userId, retrievedWebsite.UserId);
        Assert.Equal(websiteId, retrievedWebsite.Id);
    }
    
    [Fact]
    public async Task GetByIdAndUserId_ReturnsNothing_WhenQueryingOtherUserWebsite()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid otherUserId = Guid.NewGuid();
        Website website = await WebsiteScenarios.DefaultWebsite(userId, "http://website.dk");
        Guid websiteId = website.Id;

        // Act
        Website? retrievedWebsite = await _sut.GetByIdAndUserId(websiteId, otherUserId);
        
        // Assert
        Assert.Null(retrievedWebsite);
    }
    
    [Fact]
    public async Task GetByIdAndUserId_ReturnsNull_WhenWebsiteDoesNotExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        Guid nonExistentWebsiteId = Guid.NewGuid();
    
        // Act
        Website? retrievedWebsite = await _sut.GetByIdAndUserId(nonExistentWebsiteId, userId);
    
        // Assert
        Assert.Null(retrievedWebsite);
    }

    [Fact]
    public async Task DeleteByUrlAndUserId_DeletesWebsite()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string url = "http://website.dk";
        Website website = await WebsiteScenarios.DefaultWebsite(userId, url);
    
        // Act
        await _sut.DeleteByUrlAndUserId(url, userId);
    
        // Assert
        Website? deleted = await _sut.GetByIdAndUserId(website.Id, userId);
        Assert.Null(deleted);
    }
    
    [Fact]
    public async Task DeleteByUrlAndUserId_DeletesOnlyTargetWebsite()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string targetUrl = "http://website1.dk";
        string keepUrl = "http://website2.dk";

        await WebsiteScenarios.DefaultWebsite(userId, targetUrl);
        Website keepWebsite = await WebsiteScenarios.DefaultWebsite(userId, keepUrl);
    
        // Act
        await _sut.DeleteByUrlAndUserId(targetUrl, userId);
    
        // Assert
        ICollection<Website> remaining = await _sut.GetAllByUserId(userId);
        Assert.Single(remaining);
        Assert.Equal(keepUrl, remaining.First().Url);
    }
    
    // TODO: Cannot delete others website
    // TODO: Delete non-existant webstie
}