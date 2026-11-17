using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Notifications.ViewModels;

namespace OrchardCore.Notifications.Endpoints.Management;

public static class MarkAsReadEndpoints
{
    public const string RouteName = "NotificationsMarkAsRead";

    public static IEndpointRouteBuilder AddMarkAsReadEndpoint(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("Notifications/MarkAsRead", HandleAsync)
            .AllowAnonymous()
            .WithName(RouteName)
            .DisableAntiforgery();

        return builder;
    }

    private static async Task<IResult> HandleAsync(
        ReadNotificationViewModel model,
        IAuthorizationService authorizationService,
        IHttpContextAccessor httpContextAccessor,
        INotificationManager notificationManager,
        INotificationVisibilityEvaluator visibilityEvaluator)
    {
        if (string.IsNullOrEmpty(model?.MessageId))
        {
            return TypedResults.BadRequest();
        }

        var user = httpContextAccessor.HttpContext.User;

        if (!await authorizationService.AuthorizeAsync(user, NotificationPermissions.ManageNotifications))
        {
            return TypedResults.Forbid();
        }

        var notification = await notificationManager.FindVisibleForUserAsync(visibilityEvaluator, model.MessageId, user);

        if (notification == null)
        {
            return TypedResults.NotFound();
        }

        var updated = await notificationManager.MarkAsReadAsync(notification);

        return TypedResults.Ok(new
        {
            messageId = model.MessageId,
            updated,
        });
    }
}
