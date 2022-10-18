using System;
using OrchardCore.Entities;

namespace OrchardCore.Notifications;

public class WebNotification : Entity
{
    /// <summary>
    /// The Id of the notification.
    /// </summary>
    public string NotificationId { get; set; }

    /// <summary>
    /// The subject of the notification
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// The body of the notification
    /// </summary>
    public string Body { get; set; }

    /// <summary>
    /// Whether or not the body is an HTML
    /// </summary>
    public bool IsHtmlBody { get; set; }

    /// <summary>
    /// Whether or not the notification is read.
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// The date and time when the notification was read.
    /// </summary>
    public DateTime? ReadAtUtc { get; set; }

    /// <summary>
    /// The user id of the user who caused the event to occur.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The date and time when the event occurred.
    /// </summary>
    public DateTime CreatedUtc { get; set; }
}
