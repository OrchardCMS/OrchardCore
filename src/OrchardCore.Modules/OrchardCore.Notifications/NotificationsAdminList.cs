namespace OrchardCore.Notifications;

/// <summary>
/// CSS classes shared by the notifications admin list's row driver and templates.
/// </summary>
public static class NotificationsAdminList
{
    /// <summary>
    /// The CSS class carried by the row of a notification that has not been read yet. It is added to the
    /// row shape rather than to the row template, so every layout renders it on its own row element.
    /// </summary>
    public const string UnreadCssClass = "notification-is-unread";

    /// <summary>
    /// The CSS class carried by the row of a notification that has been read.
    /// </summary>
    public const string ReadCssClass = "notification-is-read";
}
