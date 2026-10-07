using System.Security.Claims;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.DTOs.Website;
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
        group.MapGet("/{id}", GetById); 
        group.MapDelete("/{id}", DeleteById);
        group.MapPost("/", Create);
        
        return group;
    }

    public static async Task<Ok<ICollection<WebsiteDTO>>> GetAll(ClaimsPrincipal user, IWebsiteService websiteService) {
        Guid userId = user.GetUserId();
        ICollection<WebsiteDTO> websites = await websiteService.GetWebsitesByUserId(userId);
    
        return TypedResults.Ok(websites);
    }

    public static async Task<Results<Ok<WebsiteDTO>, NotFound>> GetById(
            ClaimsPrincipal user, 
            Guid id, 
            IWebsiteService websiteService
            ) {
        WebsiteDTO? website = await websiteService.GetById(id);
        
        if(website is null) {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(website);
    }

    public static async Task<Results<NoContent, NotFound, UnauthorizedHttpResult>> DeleteById(ClaimsPrincipal user, Guid id, IWebsiteService websiteService) {
        WebsiteDTO? website = await websiteService.GetById(id);
        Guid userId = user.GetUserId();

        if(website is null) {
            return TypedResults.NotFound();
        } else if(website.UserId != userId) {
            return TypedResults.Unauthorized();
        }

        await websiteService.DeleteWebsite(website.Url, userId);

        return TypedResults.NoContent();
    } 

    public static async Task<Created<WebsiteDTO>> Create(
            ClaimsPrincipal user,
            CreateWebsiteRequest request, 
            IWebsiteService websiteService
            ) {
        Guid userId = user.GetUserId();
        WebsiteDTO website = await websiteService.AddWebsite(request.Url, userId, request.Name);

        return TypedResults.Created($"/websites/{website.Id}", website);
    }

}
