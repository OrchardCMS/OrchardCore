namespace OrchardCore.Email;

/// <summary>
/// Represents a contract for an SMTP email service.
/// </summary>
[Obsolete("this interface is obsolete and will be removed in future releases. Instead please use IEmailService.")]
public interface ISmtpService
{
    /// <summary>
    /// Sends the specified message to an SMTP server for delivery.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <returns>A <see cref="SmtpResult"/> that holds information about the sent message, for instance if it has sent successfully or if it has failed.</returns>
    Task<SmtpResult> SendAsync(MailMessage message);
}
