using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Entities;
using OrchardCore.Environment.Cache;
using OrchardCore.Modules;
using OrchardCore.Notifications.Indexes;
using OrchardCore.Notifications.Models;
using OrchardCore.Notifications.ViewModels;
using YesSql;

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
        ITagCache tagCache,
        YesSql.ISession session,
        IClock clock)
    {
        if (string.IsNullOrEmpty(model?.MessageId))
        {
            return TypedResults.BadRequest();
        }

        if (!await authorizationService.AuthorizeAsync(httpContextAccessor.HttpContext.User, NotificationPermissions.ManageNotifications))
        {
            return TypedResults.Forbid();
        }

        var userId = httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

        var notification = await session.Query<Notification, NotificationIndex>(x => x.NotificationId == model.MessageId && x.UserId == userId, collection: NotificationConstants.NotificationCollection).FirstOrDefaultAsync();

        if (notification == null)
        {
            return TypedResults.NotFound();
        }

        var updated = false;
        var readInfo = notification.As<NotificationReadInfo>();

        if (!readInfo.IsRead)
        {
            readInfo.ReadAtUtc = clock.UtcNow;
            readInfo.IsRead = true;
            notification.Put(readInfo);

            updated = true;

            await session.SaveAsync(notification, collection: NotificationConstants.NotificationCollection);
            await tagCache.RemoveTagAsync(NotificationsHelper.GetUnreadUserNotificationTagKey(httpContextAccessor.HttpContext.User.Identity.Name));
        }

        return TypedResults.Ok(new
        {
            messageId = model.MessageId,
            updated,
        });
    }
}
