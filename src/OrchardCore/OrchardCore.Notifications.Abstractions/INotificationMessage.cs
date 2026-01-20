namespace OrchardCore.Notifications;

/// <summary>
/// Represents a contract for notification message information.
/// </summary>
public interface INotificationMessage
{
    /// <summary>
    /// Gets the message subject. This property cannot contain HTML.
    /// </summary>
    string Subject { get; }

    /// <summary>
    /// Gets the message summary. This property contains HTML.
    /// </summary>
    string Summary { get; }

    /// <summary>
    /// Gets the plain message body. This property cannot contain HTML.
    /// </summary>
    string TextBody { get; }

    /// <summary>
    /// Gets the HTML message body. This property contains HTML.
    /// </summary>
    string HtmlBody { get; }

    /// <summary>
    /// Gets whether HTML is preferred for the message body.
    /// </summary>
    bool IsHtmlPreferred { get; }
}
