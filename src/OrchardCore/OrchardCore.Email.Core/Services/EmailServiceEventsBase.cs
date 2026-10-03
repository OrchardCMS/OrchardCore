namespace OrchardCore.Email.Services;

public class EmailServiceEventsBase : IEmailServiceEvents
{
    /// <summary>
    /// Called when an email fails to send.
    /// </summary>
    /// <param name="message">The email message that failed to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task FailedAsync(MailMessage message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called before an email is sent.
    /// </summary>
    /// <param name="message">The email message about to be sent.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task SendingAsync(MailMessage message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called after an email is sent successfully.
    /// </summary>
    /// <param name="message">The email message that was sent successfully.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task SentAsync(MailMessage message, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called after email validation completes.
    /// </summary>
    /// <param name="message">The email message that was validated.</param>
    /// <param name="context">The email validation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task ValidatedAsync(MailMessage message, MailMessageValidationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Called before email validation begins.
    /// </summary>
    /// <param name="message">The email message being validated.</param>
    /// <param name="context">The email validation context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A completed task.</returns>
    public virtual Task ValidatingAsync(MailMessage message, MailMessageValidationContext context, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
