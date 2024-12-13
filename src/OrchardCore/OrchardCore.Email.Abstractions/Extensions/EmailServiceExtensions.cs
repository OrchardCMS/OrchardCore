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
    /// <param name="body">The email body.</param>
    /// <param name="isHtmlBody">Whether the <paramref name="body"/> is in HTML format or not. Defaults to <c>true</c>.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentException"></exception>
    public static Task<EmailResult> SendAsync(this IEmailService emailService, string to, string subject, string body, bool isHtmlBody = true)
    {
        var message = new MailMessage
        {
            To = to,
            Subject = subject,
            Body = body,
            IsHtmlBody = isHtmlBody
        };

        return emailService.SendAsync(message);
    }
}
