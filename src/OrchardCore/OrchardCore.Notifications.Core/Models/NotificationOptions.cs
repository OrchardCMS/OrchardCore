namespace OrchardCore.Notifications.Models;

public class NotificationOptions
{
    /// <summary>
    /// The number of unread notifications to show in the navbar.
    /// </summary>
    public int TotalUnreadNotifications { get; set; } = 10;

    /// <summary>
    /// Whether or not to disable HtmlBody Sanitizer.
    /// </summary>
    public bool DisableNotificationHtmlBodySanitizer { get; set; }

    /// <summary>
    /// How may seconds should the top-unread notification be cache for.
    /// 0 value will indicate no cache.
    /// </summary>
    public int CacheDurationInSeconds { get; set; } = 3600;
}
