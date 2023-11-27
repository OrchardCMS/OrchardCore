using System.Threading.Tasks;

namespace OrchardCore.Email;

/// <summary>
/// Provides an extension methods to <see cref="ISmtpService"/>.
/// </summary>
public static class SmtpServiceExtensions
{
    /// <summary>
    /// Sends the specified message to an SMTP server for delivery.
    /// </summary>
    /// <param name="smtpService">The <see cref="ISmtpService"/>.</param>
    /// <param name="to">The email recipients.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <param name="isHtmlBody">Whether the <paramref name="body"/> in HTML format or not. Defaults to <c>true</c>.</param>
    /// <param name="cc">The carbon copy emails. Defaults to <c>null</c>.</param>
    /// <param name="bcc">A blind copy emails. Defaults to <c>null</c>.</param>
    /// <param name="replyTo">The replied to emails. Defaults to <c>null</c>.</param>
    /// <returns></returns>
    /// <exception cref="System.ArgumentException"></exception>
    public static async Task<SmtpResult> SendAsync(this ISmtpService smtpService, string to, string subject, string body, bool isHtmlBody = true, string cc = null, string bcc = null, string replyTo = null)
        => await smtpService.SendAsync(from: null, to, subject, body, isHtmlBody, cc, bcc, replyTo);

    /// <summary>
    /// Sends the specified message to an SMTP server for delivery.
    /// </summary>
    /// <param name="smtpService">The <see cref="ISmtpService"/>.</param>
    /// <param name="from">The email sender.</param>
    /// <param name="to">The email recipients.</param>
    /// <param name="subject">The email subject.</param>
    /// <param name="body">The email body.</param>
    /// <param name="isHtmlBody">Whether the <paramref name="body"/> in HTML format or not. Defaults to <c>true</c>.</param>
    /// <param name="cc">The carbon copy emails. Defaults to <c>null</c>.</param>
    /// <param name="bcc">A blind copy emails. Defaults to <c>null</c>.</param>
    /// <param name="replyTo">The replied to emails. Defaults to <c>null</c>.</param>
    public static async Task<SmtpResult> SendAsync(this ISmtpService smtpService, string from, string to, string subject, string body, bool isHtmlBody = true, string cc = null, string bcc = null, string replyTo = null)
    {
        var message = new MailMessage
        {
            From = from,
            To = to,
            Cc = cc,
            Bcc = bcc,
            ReplyTo = replyTo,
            Subject = subject,
            Body = body,
            IsHtmlBody = isHtmlBody
        };

        return await smtpService.SendAsync(message);
    }
}
