using Microsoft.Extensions.Localization;

namespace OrchardCore.Email;

public interface IEmailProvider
{
    /// <summary>
    /// The display name of the provider.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Send the given message via email.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <returns>EmailResult object.</returns>
    Task<EmailResult> SendAsync(MailMessage message);
}
