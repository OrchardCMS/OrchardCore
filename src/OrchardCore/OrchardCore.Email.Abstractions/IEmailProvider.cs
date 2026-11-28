using OrchardCore.Infrastructure;

namespace OrchardCore.Email;

/// <summary>
/// Sends email messages. Implement <see cref="IEmailProviderDescription"/> too, to register the provider with
/// <c>services.AddEmailProvider&lt;T&gt;(name)</c>.
/// </summary>
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
