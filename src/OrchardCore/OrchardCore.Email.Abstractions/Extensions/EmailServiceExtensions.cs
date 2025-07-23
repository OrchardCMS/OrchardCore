namespace OrchardCore.Email;

/// <summary>
/// Provides extension methods to <see cref="ISmtpService"/>.
/// </summary>
public static class EmailServiceExtensions
{
    /// <summary>
    /// Sends the specified message to an SMTP server for delivery.
    /// </summary>
    /// <param name="emailService">The <see cref="IEmailService"/>.</param>
    /// <param name="to">The email recipients.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="htmlBody">An optional email body in HTML format.</param>
    /// <param name="textBody">An optional email body in Text format.</param>
    /// <exception cref="System.ArgumentException"></exception>
    public static Task<EmailResult> SendAsync(this IEmailService emailService, string to, string subject, string htmlBody, string textBody)
    {
        var message = new MailMessage
        {
            To = to,
            Subject = subject,
            HtmlBody = htmlBody,
            TextBody = textBody,
        };

        return emailService.SendAsync(message);
    }

    /// <summary>
    /// Sends the specified message to an SMTP server for delivery.
    /// </summary>
    /// <param name="emailService">The <see cref="IEmailService"/>.</param>
    /// <param name="to">The email recipients.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <param name="isHtmlBody">Whether the <paramref name="body"/> is in HTML format or not. Defaults to <c>true</c>.</param>
    /// <exception cref="ArgumentException"></exception>
    public static async Task<EmailResult> SendAsync(this IEmailService emailService, string to, string subject, string body, bool isHtmlBody = true)
    {
        string htmlBody = default;
        string textBody = default;
        if (isHtmlBody)
        {
            htmlBody = body;
        }
        else
        {
            textBody = body;
        }

        return await emailService.SendAsync(to, subject, htmlBody, textBody);
    }
}
