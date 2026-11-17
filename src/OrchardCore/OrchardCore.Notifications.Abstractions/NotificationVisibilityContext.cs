using System.Security.Claims;

namespace OrchardCore.Notifications;

/// <summary>
/// The context of <see cref="INotificationVisibilityHandler.EvaluateAsync"/>.
/// </summary>
public sealed class NotificationVisibilityContext
{
    /// <summary>
    /// Creates a new instance of <see cref="NotificationVisibilityContext"/>.
    /// </summary>
    /// <param name="notification">The notification to evaluate.</param>
    /// <param name="user">The user the notification is shown to.</param>
    public NotificationVisibilityContext(Notification notification, ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(notification);

        Notification = notification;
        User = user;
    }

    /// <summary>
    /// Gets the notification to evaluate.
    /// </summary>
    public Notification Notification { get; }

    /// <summary>
    /// Gets the user the notification is shown to.
    /// </summary>
    public ClaimsPrincipal User { get; }

    /// <summary>
    /// Gets the visibility decided by the handlers so far. The most restrictive decision wins.
    /// </summary>
    public NotificationVisibility Visibility { get; private set; }

    /// <summary>
    /// Hides the notification: it is neither listed nor counted.
    /// </summary>
    public void Hide()
    {
        Visibility = NotificationVisibility.Hidden;
    }

    /// <summary>
    /// Restricts the notification: it is listed and counted without its content, unless another handler hides it.
    /// </summary>
    public void Restrict()
    {
        if (Visibility == NotificationVisibility.Visible)
        {
            Visibility = NotificationVisibility.Restricted;
        }
    }
}
