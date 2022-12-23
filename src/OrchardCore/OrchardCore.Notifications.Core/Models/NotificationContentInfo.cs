using System;

namespace OrchardCore.Notifications.Models;

public class NotificationReadInfo
{
    /// <summary>
    /// Whether or not the notification is read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// The date and time when the notification was read.
    /// </summary>
    public DateTime? ReadAtUtc { get; set; }
}

public class NotificationBodyInfo
{
    /// <summary>
    /// The body of the notification.
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Whether or not the body is an HTML.
    /// </summary>
    public bool IsHtmlBody { get; set; }
}
