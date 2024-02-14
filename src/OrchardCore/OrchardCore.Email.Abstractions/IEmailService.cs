using System.Threading.Tasks;

namespace OrchardCore.Email;

public interface IEmailService
{
    /// <summary>
    /// Send the given message as email.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="name">The key of the Email provider. When null or empty is provider, the default provider is used.</param>
    /// <returns>EmailResult object.</returns>
    Task<EmailResult> SendAsync(MailMessage message, string providerName = null);
}
