using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Admin;

namespace OrchardCore.Notifications.Endpoints.Management;

public static class OpenNotificationEndpoints
{
    public const string RouteName = "NotificationsOpen";

    /// <summary>
    /// Maps the endpoint the links of the notifications point to: it marks the notification as read, then
    /// redirects to its URL, or to the notification in the notification center when it has no URL.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    /// <returns>The endpoint route builder.</returns>
    public static IEndpointRouteBuilder AddOpenNotificationEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("Notifications/Open/{notificationId}", HandleAsync)
            .AllowAnonymous()
            .WithName(RouteName);

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        string notificationId,
        HttpContext httpContext,
        IAuthorizationService authorizationService,
        INotificationManager notificationManager,
        INotificationVisibilityEvaluator visibilityEvaluator,
        LinkGenerator linkGenerator)
    {
        var user = httpContext.User;

        if (user.Identity?.IsAuthenticated != true)
        {
            return TypedResults.Challenge();
        }

        if (!await authorizationService.AuthorizeAsync(user, NotificationPermissions.ManageNotifications))
        {
            return TypedResults.Forbid();
        }

        var notification = await notificationManager.FindByIdAsync(notificationId, httpContext.RequestAborted);

        if (notification == null || notification.UserId != user.FindFirstValue(ClaimTypes.NameIdentifier))
        {
            return TypedResults.NotFound();
        }

        var visibility = await visibilityEvaluator.EvaluateAsync(notification, user, httpContext.RequestAborted);

        if (visibility == NotificationVisibility.Hidden)
        {
            return TypedResults.NotFound();
        }

        await notificationManager.MarkAsReadAsync(notification, httpContext.RequestAborted);

        if (visibility == NotificationVisibility.Visible)
        {
            var url = NotificationUrlHelper.Resolve(notification.Url, httpContext.Request.PathBase);

            if (!string.IsNullOrEmpty(url))
            {
                return TypedResults.Redirect(url);
            }
        }

        if (await authorizationService.AuthorizeAsync(user, AdminPermissions.AccessAdminPanel))
        {
            var displayUrl = linkGenerator.GetPathByAction(httpContext, "Display", "Admin", new
            {
                area = NotificationConstants.Features.Notifications,
                notificationId = notification.NotificationId,
            });

            if (!string.IsNullOrEmpty(displayUrl))
            {
                return TypedResults.Redirect(displayUrl);
            }
        }

        return TypedResults.Redirect($"{httpContext.Request.PathBase}/");
    }
}
