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
}
