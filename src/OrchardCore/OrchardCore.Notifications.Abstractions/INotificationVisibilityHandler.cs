namespace OrchardCore.Notifications;

/// <summary>
/// Decides whether the current user may still see a notification, each time the notification is shown:
/// in the navbar, in its unread count, in the notification center and when the notification is opened.
/// </summary>
/// <remarks>
/// <para>
/// Implement it when a user can lose access to the record a notification is about after the notification was
/// sent, for example when they are removed from a project team. Every handler is called for every shown
/// notification: skip the notifications of other types, and load what the others need at once rather than per notification.
/// </para>
/// <para>
/// A handler is a safety net that runs on every display. When the change is known, for example when a user
/// leaves the team, also delete or update the notifications with <see cref="INotificationManager"/>, so they
/// are not loaded only to be hidden.
/// </para>
/// </remarks>
public interface INotificationVisibilityHandler
{
    /// <summary>
    /// Evaluates the visibility of notifications for the current user, and calls
    /// <see cref="NotificationVisibilityContext.Hide"/> or <see cref="NotificationVisibilityContext.Restrict"/> to limit them.
    /// When the method throws, all these notifications are hidden.
    /// </summary>
    /// <param name="contexts">The visibility context of each notification, all for the same user. It is never empty.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the visibility is evaluated.</returns>
    Task EvaluateAsync(IReadOnlyList<NotificationVisibilityContext> contexts, CancellationToken cancellationToken = default);
}
