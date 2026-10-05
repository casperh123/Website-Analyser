using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.Endpoints;
using WebsiteAnalyzer.Application.Services;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Interfaces.Repositories;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Website;
using WebsiteAnalyzer.Infrastructure.Repositories;
using WebsiteAnalyzer.TestUtilities.Builders;
using WebsiteAnalyzer.TestUtilities.Database;
using WebsiteAnalyzer.TestUtilities.Testing;
using Xunit;
using Assert = Xunit.Assert;

namespace WebsiteAnalyzer.Api.Test.WebsiteTests;

public class WebsiteEndpointsUnitTests : TestBase
{
    private readonly IWebsiteService _websiteService;

    public WebsiteEndpointsUnitTests(DatabaseFixture fixture) : base(fixture)
    {
        IWebsiteRepository websiteRepository = new WebsiteRepository(DbContext);
        IScheduledActionRepository scheduledActionRepository = new ScheduledActionRepository(DbContext);
        IScheduleService scheduleService = new ScheduleService(scheduledActionRepository, websiteRepository);
        HttpClient client = new HttpClient();
        _websiteService = new WebsiteService(websiteRepository, scheduleService, client);
    }

    [Fact]
    public async Task GetById_Returns_OKWebsite()
    {
        // Arrange
        (ClaimsPrincipal user, Guid userId) = new ClaimsBuilder().Default();
        string websiteUrl = "https://example.com";
        Website website = await WebsiteScenarios.CreateDefault(userId, websiteUrl);
        
        // Act
        Results<Ok<WebsiteDTO>, NotFound> result = 
            await WebsiteEndpoints.GetById(
                user,
                website.Id,
                _websiteService
                );

        // Assert
        Assert.IsType<Ok<WebsiteDTO>>(result.Result);
    }
    
    [Fact]
    public async Task GetById_Returns_NotFound()
    {
        // Arrange
        (ClaimsPrincipal user, Guid userId) = new ClaimsBuilder().Default();
        Guid websiteId = Guid.NewGuid();
        
        // Act
        Results<Ok<WebsiteDTO>, NotFound> result = 
            await WebsiteEndpoints.GetById(
                user,
                websiteId,
                _websiteService
            );

        // Assert
        Assert.IsType<NotFound>(result.Result);
    }
}