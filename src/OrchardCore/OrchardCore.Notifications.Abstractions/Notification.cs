using OrchardCore.Entities;

namespace OrchardCore.Notifications;

/// <summary>
/// Represents a notification entity.
/// </summary>
public class Notification : Entity
{
    /// <summary>
    /// Gets or sets the notification Id.
    /// </summary>
    public string NotificationId { get; set; }

    /// <summary>
    /// Gets or sets the notification subject.
    /// </summary>
    public string Subject { get; set; }

    /// <summary>
    /// Gets or sets the notification summary.
    /// </summary>
    public string Summary { get; set; }

    /// <summary>
    /// Gets or sets the user id who caused the event to occur.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the event occurred.
    /// </summary>
    public DateTime CreatedUtc { get; set; }
}
