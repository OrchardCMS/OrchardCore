namespace OrchardCore.Email;

/// <summary>
/// Represents a body for <see cref="MailMessage"/>.
/// </summary>
public class MailMessageBody
{
    /// <summary>
    /// Gets or sets the body in plain text format.
    /// </summary>
    public string PlainText { get; set; }

    /// <summary>
    /// Gets or sets the body in HTML format.
    /// </summary>
    public string Html { get; set; }

    public static implicit operator MailMessageBody(string body) => new()
    {
        Html = body,
        PlainText = body
    };
}
