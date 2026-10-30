using System.Security.Claims;

namespace OrchardCore.Notifications;

/// <summary>
/// Evaluates the visibility of notifications for a user with the registered <see cref="INotificationVisibilityHandler"/>
/// implementations. The decisions are kept for the rest of the request.
/// </summary>
public interface INotificationVisibilityEvaluator
{
    /// <summary>
    /// Gets whether a <see cref="INotificationVisibilityHandler"/> is registered. Every notification is
    /// visible when none is, so the notifications can be counted and paged by the database.
    /// </summary>
    bool HasHandlers { get; }

    /// <summary>
    /// Gets the visibility of a notification for a user.
    /// </summary>
    /// <param name="notification">The notification.</param>
    /// <param name="user">The user the notification is shown to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The visibility of the notification.</returns>
    Task<NotificationVisibility> EvaluateAsync(Notification notification, ClaimsPrincipal user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the hidden notifications from a list.
    /// </summary>
    /// <param name="notifications">The notifications.</param>
    /// <param name="user">The user the notifications are shown to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The notifications that are not hidden, in the same order.</returns>
    Task<List<Notification>> FilterAsync(IEnumerable<Notification> notifications, ClaimsPrincipal user, CancellationToken cancellationToken = default);
}
