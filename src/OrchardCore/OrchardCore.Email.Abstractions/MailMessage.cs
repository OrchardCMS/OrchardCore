namespace OrchardCore.Email;

/// <summary>
/// Represents a class that contains information of the mail message.
/// </summary>
public class MailMessage
{
    /// <summary>
    /// Gets or sets the author of the email.
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// Gets or sets the recipients.
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// Gets or sets the carbon copy emails.
    /// </summary>
    public string Cc { get; set; }

    /// <summary>
    /// Gets or sets a blind copy emails.
    /// </summary>
    public string Bcc { get; set; }

    /// <summary>
    /// Gets or sets the replied to emails.
    /// </summary>
    public string ReplyTo { get; set; }

    /// <summary>
    /// Gets or sets the actual submitter of the email.
    /// </summary>
    /// <remark>
    /// This property is required if not the same as <see cref="From"/>, for more information please refer to https://ietf.org/rfc/rfc822.txt.
    /// </remark>
    public string Sender { get; set; }

    /// <summary>
    /// Gets or sets the message subject.
    /// </summary>
    public string Subject { get; set; }

    [Obsolete("This property has been deprecated, please use the HtmlBody or TextBody property instead.")]
    public bool IsHtmlBody { get; set; }

    [Obsolete("This property has been deprecated, please use the HtmlBody or TextBody property instead.")]
    public string Body { get; set; }

    /// <summary>
    /// Gets or sets the message content in HTML format.
    /// </summary>
    public string HtmlBody { get; set; }

    /// <summary>
    /// Gets or sets the message content in plain text format.
    /// </summary>
    public string TextBody { get; set; }

    /// <summary>
    /// The collection of message attachments.
    /// </summary>
    public List<MailMessageAttachment> Attachments { get; init; } = [];
}
