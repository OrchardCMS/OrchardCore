using Annotype;
using OrchardCore.Infrastructure;

namespace OrchardCore.Email;

/// <summary>
/// Sends email messages.
/// </summary>
/// <remarks>
/// Every concrete implementation must declare an <see cref="EmailProviderAttribute"/>, which is enforced at compile time.
/// </remarks>
[RequiresMetadata(typeof(EmailProviderAttribute))]
public interface IEmailProvider
{
    /// <summary>
    /// Send the given message via email.
    /// </summary>
    /// <param name="message">The email message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the email was sent successfully.</returns>
    Task<Result> SendAsync(MailMessage message, CancellationToken cancellationToken = default);
}
