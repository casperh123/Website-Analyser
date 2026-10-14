using System.Security.Claims;
using Crawl.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.DTOs;
using WebsiteAnalyser.Api.Streaming;
using WebsiteAnalyser.Api.User;
using WebsiteAnalyzer.Core.Contracts.BrokenLink;
using WebsiteAnalyzer.Core.Domain;
using WebsiteAnalyzer.Core.Domain.BrokenLink;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Website;

namespace WebsiteAnalyser.Api.Endpoints;

public static class BrokenLinkEndpoints {
    public static RouteGroupBuilder MapBrokenLinkEndpoints(this IEndpointRouteBuilder app) {
        RouteGroupBuilder route = app.MapGroup("/")
                                        .RequireAuthorization();

        route.MapGet("/websites/{id}/broken-links", Get);
        route.MapPost("/websites/{id}/broken-links", StartCrawl);

        return route;
    }

    public static async Task<Ok<PagedResponse<BrokenLinkCrawlDTO>>> Get(
            ClaimsPrincipal user, 
            Guid id, 
            IBrokenLinkService brokenLinkService, 
            IWebsiteService websiteService,
            int page = 1, 
            int perPage = 20) {
        Guid userId = user.GetUserId();
        WebsiteDTO? website = await websiteService.GetById(id);

        IEnumerable<BrokenLinkCrawlDTO> crawls = await brokenLinkService.GetBrokenLinkCrawlsByUrlAndUserId(website?.Url, userId);
        IEnumerable<BrokenLinkCrawlDTO> crawlPage = crawls
                                                .Skip((page - 1) * perPage)
                                                .Take(perPage);

        PagedResponse<BrokenLinkCrawlDTO> pagedCrawls = new(crawlPage, page, perPage, crawls.Count());

        return TypedResults.Ok(pagedCrawls);
    }

    public static async Task<ServerSentEventsResult<ProgressReport>> StartCrawl(IBrokenLinkService brokenLinkService, Guid id, CancellationToken cancellationToken) {
        return TypedResults.ServerSentEvents(
            StreamCrawl(id, brokenLinkService, cancellationToken),
            eventType: "crawl-update"
        );
    }

    private static async IAsyncEnumerable<ProgressReport> StreamCrawl(Guid id, IBrokenLinkService brokenLinkService, CancellationToken cancellationToken) {
        ThrottledProgress<CrawlProgress> progress = new(TimeSpan.FromSeconds(3));

        Task crawlTask = progress.RunAsync(async () => {
            await brokenLinkService.FindBrokenLinks(id, progress, cancellationToken);
        });

        await foreach(CrawlProgress update in progress.ReadAllAsync(cancellationToken)){
        
            yield return new ProgressReport {
                LinksCrawled = update.TotalCrawled,
                LinksEnqueued = update.QueueSize
            };
        }

        await crawlTask;
    }
}
