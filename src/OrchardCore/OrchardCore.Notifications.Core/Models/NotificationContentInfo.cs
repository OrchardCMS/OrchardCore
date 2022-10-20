using System;

namespace OrchardCore.Notifications.Models;

public class NotificationContentInfo
{
    /// <summary>
    /// ContentItemId this notification is linked to.
    /// </summary>
    public string ContentItemId { get; set; }

    /// <summary>
    /// The type of the referenced contentItem.
    /// </summary>
    public string ContentType { get; set; }

    /// <summary>
    /// The owner of the referenced contentItem.
    /// </summary>
    public string ContentOwnerId { get; set; }

    /// <summary>
    /// The link type of the notification.
    /// </summary>
    public NotificationLinkType LinkType { get; set; }

    /// <summary>
    /// A custom URL to send then user too when the notification is clicked.
    /// </summary>
    public string CustomUrl { get; set; }
}

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
