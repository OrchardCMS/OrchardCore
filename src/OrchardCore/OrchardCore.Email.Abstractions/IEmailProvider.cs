using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Email;

public interface IEmailProvider
{
    /// <summary>
    /// The name of the provider.
    /// </summary>
    LocalizedString Name { get; }

    /// <summary>
    /// Send the given message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>EmailResult object.</returns>
    Task<EmailResult> SendAsync(MailMessage message);
}
