using System.Threading.Tasks;

namespace OrchardCore.Email;

/// <summary>
/// Represents a contract for email service.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends the specified message to email server for delivery.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <returns>A <see cref="EmailResult"/> that holds information about the sent message, for instance if it has sent successfully or if it has failed.</returns>
    Task<EmailResult> SendAsync(MailMessage message);
}
