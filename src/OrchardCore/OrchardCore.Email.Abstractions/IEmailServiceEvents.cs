namespace OrchardCore.Email;

public interface IEmailServiceEvents
{
    /// <summary>
    /// This event is triggered during the email validation and before sending the message.
    /// </summary>
    Task ValidatingAsync(MailMessage message, MailMessageValidationContext context);

    /// <summary>
    /// After the email validation process is concluded, this event is activated.
    /// It will be triggered regardless of whether the validation was successful or not.
    /// </summary>
    Task ValidatedAsync(MailMessage message, MailMessageValidationContext context);

    /// <summary>
    /// This event is triggered before sending the email via the email provider.
    /// </summary>
    Task SendingAsync(MailMessage message);

    /// <summary>
    /// This event is triggered after the email was successfully sent.
    /// </summary>
    Task SentAsync(MailMessage message);

    /// <summary>
    /// This event is triggered if the email fails to send.
    /// </summary>
    Task FailedAsync(MailMessage message);
}
