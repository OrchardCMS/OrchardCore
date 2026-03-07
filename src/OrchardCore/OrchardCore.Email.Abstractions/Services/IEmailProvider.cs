using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure;

namespace OrchardCore.Email.Services;

public interface IEmailProvider
{
    string Name { get; }

    /// <summary>
    /// The display name of the provider.
    /// </summary>
    LocalizedString DisplayName { get; }

    /// <summary>
    /// Send the given message via email.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <returns>EmailResult object.</returns>
    Task<Result> SendAsync(MailMessage message);
}
