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

    /// <summary>
    /// Gets the kind of notification, for example <c>Mention</c>. Optional.
    /// </summary>
    /// <remarks>
    /// Register the type with <c>services.AddNotificationType()</c> to give it a display name, list it in the
    /// type filter of the notification center and let users choose their notification methods for it.
    /// The value is limited to 64 characters.
    /// </remarks>
    string Type => null;

    /// <summary>
    /// Gets the identifier of the record the notification is about, for example the id of a comment. Optional.
    /// </summary>
    /// <remarks>The value is limited to 255 characters.</remarks>
    string ReferenceId => null;

    /// <summary>
    /// Gets the URL the notification points to. Optional.
    /// </summary>
    /// <remarks>
    /// Use an application-relative URL starting with <c>~/</c> for a page of the tenant, or an absolute
    /// <c>http</c> or <c>https</c> URL. Opening the notification marks it as read, then redirects to this URL.
    /// </remarks>
    string Url => null;
}
