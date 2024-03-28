namespace OrchardCore.Email;

/// <summary>
/// Represents a body for <see cref="MailMessage"/>.
/// </summary>
public class MailMessageBody
{
    /// <summary>
    /// Gets or sets the body in plain text format.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the body in HTML format.
    /// </summary>
    public string Html { get; set; }
}
