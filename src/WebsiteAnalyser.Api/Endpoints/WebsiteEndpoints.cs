using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.User;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Website;

namespace WebsiteAnalyser.Api.Endpoints;

public static class WebsiteEndpoints
{
    public static RouteGroupBuilder MapWebsiteEndpoints(this IEndpointRouteBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/websites")
                                        .RequireAuthorization();

        group.MapGet("/", GetAll);
        
        return group;
    }

    public static async Task<Ok<ICollection<WebsiteDTO>>> GetAll(ClaimsPrincipal user, IWebsiteService websiteService) {
        Guid userId = user.GetUserId();
        ICollection<WebsiteDTO> websites = await websiteService.GetWebsitesByUserId(userId);
    
        return TypedResults.Ok(websites);
    }

}
