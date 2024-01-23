using System.Threading.Tasks;

namespace OrchardCore.Email.Services;

/// <summary>
/// Represents a contract for email service.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends the specified message to email server for delivery.
    /// </summary>
    /// <param name="message">The message to be sent.</param>
    /// <param name="deliveryMethodName">The delivery method name to be used to send the email. If no method specified the last <see cref="IEmailDeliveryService"/> will be used to deliver the email.</param>
    /// <returns>A <see cref="EmailResult"/> that holds information about the sent message, for instance, if it was sent successfully or if it has failed.</returns>
    /// <remarks>By default we are using <see cref="EmailDeliveryServiceName"/> to choose the email delivery service name.</remarks>
    Task<EmailResult> SendAsync(MailMessage message, string deliveryMethodName = null);
}
