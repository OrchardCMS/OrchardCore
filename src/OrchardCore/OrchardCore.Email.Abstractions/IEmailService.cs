using OrchardCore.Infrastructure;

namespace OrchardCore.Email;

public interface IEmailService
{
    /// <summary>
    /// Send the given message as email.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="providerName">The technical name of the Email provider. When null or empty, the default provider is used.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the email was sent successfully.</returns>
    Task<Result> SendAsync(MailMessage message, string providerName = null, CancellationToken cancellationToken = default);
}
