using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure;

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
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the email was sent successfully.</returns>
    Task<Result> SendAsync(MailMessage message, CancellationToken cancellationToken = default);
}
