using Microsoft.AspNetCore.Http.HttpResults;
using WebsiteAnalyser.Api.DTOs.Notifications;
using WebsiteAnalyzer.Core.Interfaces.Services;
using WebsiteAnalyzer.Core.Interfaces.Services.DTOs.Email;

namespace WebsiteAnalyser.Api.Endpoints;

public static class EmailSubscriptionEndpoints {
    public static RouteGroupBuilder MapEmailSubscriptionEndpoints(this IEndpointRouteBuilder app) {
        RouteGroupBuilder route = app.MapGroup("/subscriptions")
                                        .RequireAuthorization();

        route.MapGet("/website/{id}", GetByWebsite);
        route.MapPost("/", Create);
        route.MapDelete("/", Delete);

        return route;
    }

    public static async Task<Ok<ICollection<EmailSubscriptionDTO>>> GetByWebsite(Guid id, IEmailSubscriptionService subscriptionService) {
        ICollection<EmailSubscriptionDTO> subscriptions = await subscriptionService.GetSubscriptionsByWebsite(id);

        return TypedResults.Ok(subscriptions);
    }

    public static async Task<Ok<EmailSubscriptionDTO>> Create(CreateEmailSubscription createSubscription, IEmailSubscriptionService subscriptionService) {
        EmailSubscriptionDTO subscription = await subscriptionService.Subscribe(
                    createSubscription.WebsiteId,
                    createSubscription.ScheduledActionId,
                    createSubscription.Email
                );

        return TypedResults.Ok(subscription);
    }

    public static async Task<NoContent> Delete(Guid id, IEmailSubscriptionService subscriptionService) {
        await subscriptionService.Unsubscribe(id);

        return TypedResults.NoContent();
    }
}
