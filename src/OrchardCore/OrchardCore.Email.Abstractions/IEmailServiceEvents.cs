namespace OrchardCore.Email;

public interface IEmailServiceEvents
{
    /// <summary>
    /// This event is triggered during the email validation and before sending the message.
    /// </summary>
    /// <param name="message">The email message being validated.</param>
    /// <param name="context">The email validation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the validation callback finishes.</returns>
    Task ValidatingAsync(MailMessage message, MailMessageValidationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// After the email validation process is concluded, this event is activated.
    /// It will be triggered regardless of whether the validation was successful or not.
    /// </summary>
    /// <param name="message">The email message that was validated.</param>
    /// <param name="context">The email validation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the validated callback finishes.</returns>
    Task ValidatedAsync(MailMessage message, MailMessageValidationContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// This event is triggered before sending the email via the email provider.
    /// </summary>
    /// <param name="message">The email message about to be sent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the sending callback finishes.</returns>
    Task SendingAsync(MailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// This event is triggered after the email was successfully sent.
    /// </summary>
    /// <param name="message">The email message that was sent successfully.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the sent callback finishes.</returns>
    Task SentAsync(MailMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// This event is triggered if the email fails to send.
    /// </summary>
    /// <param name="message">The email message that failed to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that completes when the failure callback finishes.</returns>
    Task FailedAsync(MailMessage message, CancellationToken cancellationToken = default);
}
