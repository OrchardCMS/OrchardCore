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
