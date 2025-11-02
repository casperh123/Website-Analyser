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
        _sut = new WebsiteRepository(DbContext);
    }

    [Theory]
    [InlineData(3, 0)]
    [InlineData(3, 1)]
    [InlineData(10, 9)]
    [InlineData(0, 5)]
    [InlineData(0, 0)]
    public async Task GetAllByUserId_ReturnsExpectedWebsitesForEachUser(int userWebsitesCount, int otherUserWebsitesCount)
    {
        // Arrange
        (Guid userId, Guid otherUserId) = TwoIds();

        await WebsiteScenarios.CreateMultipleForUser(userId, userWebsitesCount);
        await WebsiteScenarios.CreateMultipleForUser(otherUserId, otherUserWebsitesCount);
        
        // Act
        ICollection<Website> retrievedUserWebsites = await _sut.GetAllByUserId(userId);
        ICollection<Website> retrievedOtherUserWebsites = await _sut.GetAllByUserId(otherUserId);
        
        Assert.Equal(userWebsitesCount, retrievedUserWebsites.Count);
        Assert.Equal(otherUserWebsitesCount, retrievedOtherUserWebsites.Count);
        Assert.All(retrievedUserWebsites, w => Assert.Equal(userId, w.UserId));
        Assert.All(retrievedOtherUserWebsites, w => Assert.Equal(otherUserId, w.UserId));
    }
    
    [Theory]
    [InlineData(1, 0)]
    [InlineData(3, 0)]
    [InlineData(3, 3)]
    public async Task GetByIdAndUserId_ReturnsCorrectWebsite(int userWebsiteCount, int otherUserWebsiteCount)
    {
        // Arrange
        (Guid userId, Guid otherUserId) = TwoIds();

        ICollection<Website> userWebsites = await WebsiteScenarios.CreateMultipleForUser(userId, userWebsiteCount);
        ICollection<Website> otherUserWebsites =
            await WebsiteScenarios.CreateMultipleForUser(otherUserId, otherUserWebsiteCount);

        Website websiteToFetch = userWebsites.First();
        
        // Act
        Website? retrievedWebsite = await _sut.GetByIdAndUserId(websiteToFetch.Id, userId);

        // Assert
        Assert.NotNull(retrievedWebsite);
        Assert.Equal(userId, retrievedWebsite.UserId);
        Assert.Equal(websiteToFetch.Id, retrievedWebsite.Id);
    }
    
    [Fact]
    public async Task GetByIdAndUserId_ReturnsNothing_WhenQueryingOtherUserWebsite()
    {
        // Arrange
        (Guid userId, Guid otherUserId) = TwoIds();
        Website website = await WebsiteScenarios.CreateDefault(userId, "http://website.dk");
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
        Website website = await WebsiteScenarios.CreateDefault(userId, url);
    
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

        await WebsiteScenarios.CreateDefault(userId, targetUrl);
        await WebsiteScenarios.CreateDefault(userId, keepUrl);
    
        // Act
        await _sut.DeleteByUrlAndUserId(targetUrl, userId);
    
        // Assert
        ICollection<Website> remaining = await _sut.GetAllByUserId(userId);
        Assert.Single(remaining);
        Assert.Equal(keepUrl, remaining.First().Url);
    }
    
    [Fact]
    public async Task DeleteByUrlAndUserId_DeletesOnlyUserWebsite()
    {
        // Arrange
        (Guid userId, Guid otherUserId) = TwoIds();
        string userWebsiteUrl = "http://website1.dk";
        string otherUserWebsiteUrl = "http://website2.dk";

        await WebsiteScenarios.CreateDefault(userId, userWebsiteUrl);
        Website websiteThatShouldPersist =await WebsiteScenarios.CreateDefault(otherUserId, otherUserWebsiteUrl);
        
        // Act
        await _sut.DeleteByUrlAndUserId(otherUserWebsiteUrl, userId);

        Website? retrievedWebsite = await _sut.GetByIdAndUserId(websiteThatShouldPersist.Id, otherUserId);

        // Assert
        Assert.NotNull(retrievedWebsite);
        Assert.Equal(otherUserId, retrievedWebsite.UserId);
        Assert.Equal(otherUserWebsiteUrl, retrievedWebsite.Url);
    }
    
    [Fact]
    public async Task DeleteByUrlAndUserId_CanOnlyDeleteUserWebsites()
    {
        // Arrange
        (Guid userId, Guid otherUserId) = TwoIds();
        string websiteUrl = "http://website1.dk";

        await WebsiteScenarios.CreateDefault(userId, websiteUrl);
        Website websiteThatShouldPersist = await WebsiteScenarios.CreateDefault(otherUserId, websiteUrl);
        
        // Act
        await _sut.DeleteByUrlAndUserId(websiteUrl, userId);

        Website? retrievedWebsite = await _sut.GetByIdAndUserId(websiteThatShouldPersist.Id, otherUserId);

        // Assert
        Assert.NotNull(retrievedWebsite);
        Assert.Equal(otherUserId, retrievedWebsite.UserId);
        Assert.Equal(websiteUrl, retrievedWebsite.Url);
    }
    
    [Fact]
    public async Task DeleteByUrlAndUserId_Suceeds_WhenWebsiteDoesntExist()
    {
        // Arrange
        Guid userId = Guid.NewGuid();
        string websiteUrl = "http://website.dk";
        
        // Act
        await _sut.DeleteByUrlAndUserId(websiteUrl, userId);
    }
}