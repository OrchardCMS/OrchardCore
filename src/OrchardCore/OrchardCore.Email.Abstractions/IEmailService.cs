using System.Threading.Tasks;

namespace OrchardCore.Email;

public interface IEmailService
{
    /// <summary>
    /// Send the given message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>EmailResult object.</returns>
    Task<EmailResult> SendAsync(MailMessage message);
}
