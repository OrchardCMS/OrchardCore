using System;
using OrchardCore.Entities;

namespace OrchardCore.Notifications;

public class Notification : Entity
{
    /// <summary>
    /// The Id of the notification.
    /// </summary>
    public string NotificationId { get; set; }

    /// <summary>
    /// The summary of the notification
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// The user id of the user who caused the event to occur.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// The date and time when the event occurred.
    /// </summary>
    public DateTime CreatedUtc { get; set; }
}
