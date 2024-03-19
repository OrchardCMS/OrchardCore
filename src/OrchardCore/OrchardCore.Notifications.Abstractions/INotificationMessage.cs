namespace OrchardCore.Notifications;

/// <summary>
/// Represents a contract for notification message information.
/// </summary>
public interface INotificationMessage
{
    /// <summary>
    /// Gets the message summary.
    /// </summary>
    string Summary { get; }

    /// <summary>
    /// Gets the plain message body.
    /// </summary>
    string TextBody { get; }

    /// <summary>
    /// Gets the HTML message body.
    /// </summary>
    string HtmlBody { get; }

    /// <summary>
    /// Gets whether HTML is preferred for the message body.
    /// </summary>
    bool IsHtmlPreferred { get; }
}
