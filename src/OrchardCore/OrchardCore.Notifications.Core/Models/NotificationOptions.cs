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
    /// Gets or sets an absolute expiration time, relative to now.
    /// </summary>
    public int AbsoluteCacheExpirationSeconds { get; set; }

    /// <summary>
    /// Gets or sets how long a cache entry can be inactive (e.g. not accessed) before it will be removed.
    /// This will not extend the entry lifetime beyond the absolute expiration (if set in <see cref="AbsoluteCacheExpirationSeconds" />).
    /// </summary>
    public int SlidingCacheExpirationSeconds { get; set; } = 1800;
}
