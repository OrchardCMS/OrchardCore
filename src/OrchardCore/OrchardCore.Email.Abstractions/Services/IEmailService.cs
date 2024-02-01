using System.Threading.Tasks;

namespace OrchardCore.Email.Services;

/// <summary>
/// Represents a contract for email service.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends the specified message in email.
    /// </summary>
    /// <param name="message">The message to be sent in email.</param>
    /// <param name="deliveryServiceName">The name of the delivery service to send the email. If no name is specified then `IEmailDeliveryServiceResolver` will select the last registered one.</param>
    /// <returns>An <see cref="EmailResult"/> that holds information about the message sent, for instance, if it was sent successfully or if it has failed.</returns>
    Task<IEmailResult> SendAsync(MailMessage message, string deliveryServiceName = null);
}
