using System.Security.Claims;

namespace OrchardCore.Notifications;

internal static class NotificationManagerExtensions
{
    /// <summary>
    /// Gets a notification of a user, whether it is hidden or not.
    /// </summary>
    /// <returns>The notification, or <see langword="null"/> when none has this id or it was sent to another user.</returns>
    public static async Task<Notification> FindForUserAsync(this INotificationManager manager, string notificationId, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var notification = await manager.FindByIdAsync(notificationId, cancellationToken);
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        return userId != null && notification?.UserId == userId ? notification : null;
    }

    /// <summary>
    /// Gets a notification a user can see.
    /// </summary>
    /// <returns>The notification, or <see langword="null"/> when none has this id, it was sent to another user or it is hidden.</returns>
    public static async Task<Notification> FindVisibleForUserAsync(this INotificationManager manager, INotificationVisibilityEvaluator visibilityEvaluator, string notificationId, ClaimsPrincipal user, CancellationToken cancellationToken = default)
    {
        var notification = await manager.FindForUserAsync(notificationId, user, cancellationToken);

        return notification != null && await visibilityEvaluator.EvaluateAsync(notification, user, cancellationToken) != NotificationVisibility.Hidden
            ? notification
            : null;
    }
}
