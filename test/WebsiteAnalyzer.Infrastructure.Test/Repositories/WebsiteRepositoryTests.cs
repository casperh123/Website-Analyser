using WebsiteAnalyzer.Core.Domain.Website;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Scenarios;
using WebsiteAnalyzer.TestUtilities.Testing;

namespace WebsiteAnalyzer.Infrastructure.Test.Repositories;

public class WebsiteRepositoryTests : TestBase
{
    private readonly WebsiteScenarios _websiteScenarios;
    private readonly IWebsiteRepository _websiteRepository; 

    public WebsiteRepositoryTests(DatabaseFixture fixture) : base(fixture)
    {
        _websiteScenarios = new WebsiteScenarios(Context);
        _websiteRepository = new WebsiteRepository(Context);
    }

    [Fact]
    public async Task GetAllByUserId_ReturnsUserWebsites()
    {
        //Arrange
        Guid userId = Guid.NewGuid();

        string website1Url = "http://website1.dk";
        string website2Url = "http://website2.dk";
        string website3Url = "http://website3.dk";
        
        Website[] websites = [
            await _websiteScenarios.DefaultWebsite(userId, website1Url),
            await _websiteScenarios.DefaultWebsite(userId, website2Url),
            await _websiteScenarios.DefaultWebsite(userId, website3Url)
        ];

        //Act
        ICollection<Website> retrievedWebsite = await _websiteRepository.GetAllByUserId(userId);

        //Assert
        Assert.Equal(3, retrievedWebsite.Count);
        Assert.All(retrievedWebsite, w => Assert.Equal(userId, w.UserId));
        Assert.Contains(retrievedWebsite, w => w.Url == website1Url);
        Assert.Contains(retrievedWebsite, w => w.Url == website2Url);
        Assert.Contains(retrievedWebsite, w => w.Url == website3Url);
    }

    public async Task GetAllByUserId_ReturnsOnlyUserWebsites()
    {
        //Arrange
        Guid userId = Guid.NewGuid();
    }
}