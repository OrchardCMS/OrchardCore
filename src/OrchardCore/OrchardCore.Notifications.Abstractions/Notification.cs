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
    /// Gets or sets the kind of notification, for example <c>Mention</c>, or <see langword="null"/> when the
    /// sender did not set one. Notifications are filtered by type in the notification center, a type selects
    /// the display drivers and templates of a notification, and users choose their notification methods per type.
    /// </summary>
    public string Type { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the record the notification is about, for example the id of a comment,
    /// or <see langword="null"/>. Together with <see cref="Type"/> it lets the sender find the notifications
    /// of a record again, to update or delete them when the record changes.
    /// </summary>
    public string ReferenceId { get; set; }

    /// <summary>
    /// Gets or sets the URL the notification points to, or <see langword="null"/>. It is either an absolute
    /// <c>http</c> or <c>https</c> URL, or an application-relative URL starting with <c>~/</c> that is resolved
    /// against the base path of the tenant when the notification is opened.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the user id who caused the event to occur.
    /// </summary>
    public string UserId { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the event occurred.
    /// </summary>
    public DateTime CreatedUtc { get; set; }
}
