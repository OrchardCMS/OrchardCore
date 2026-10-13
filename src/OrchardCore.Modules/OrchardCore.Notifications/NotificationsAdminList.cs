namespace OrchardCore.Notifications;

/// <summary>
/// The admin list of notifications rendered by the <c>AdminList</c> shape.
/// Its columns are declared by <see cref="NotificationsAdminListColumnDriver"/>.
/// </summary>
public static class NotificationsAdminList
{
    /// <summary>
    /// The name of the list, passed to its column drivers and used by the
    /// <c>AdminList__Notifications</c> and <c>AdminListCell__Notifications__{Column}</c> alternates.
    /// </summary>
    public const string Name = "Notifications";

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
