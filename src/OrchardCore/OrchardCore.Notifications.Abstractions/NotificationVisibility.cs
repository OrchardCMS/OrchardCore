namespace OrchardCore.Notifications;

/// <summary>
/// How a notification is shown to the current user, see <see cref="INotificationVisibilityHandler"/>.
/// </summary>
public enum NotificationVisibility
{
    /// <summary>
    /// The notification is shown with its content.
    /// </summary>
    Visible,

    /// <summary>
    /// The notification is listed and counted, but its subject, summary, body and URL are replaced by a
    /// message telling the user they can no longer see it. The user can still mark it as read or delete it.
    /// </summary>
    Restricted,

    /// <summary>
    /// The notification is neither listed nor counted, and cannot be opened.
    /// </summary>
    Hidden,
}
