using OrchardCore.Entities;

namespace OrchardCore.Notifications.Models;

/// <summary>
/// A notification message. The properties stored on the message with <c>Put()</c> are copied to the
/// <see cref="Notification"/> stored in the notification center, so its display drivers can show them.
/// </summary>
public class NotificationMessage : Entity, INotificationMessage
{
    public string Subject { get; set; }

    public string Summary { get; set; }

    public string TextBody { get; set; }

    public string HtmlBody { get; set; }

    public bool IsHtmlPreferred { get; set; }

    /// <inheritdoc/>
    public string Type { get; set; }

    /// <inheritdoc/>
    public string ReferenceId { get; set; }

    /// <inheritdoc/>
    public string Url { get; set; }
}
