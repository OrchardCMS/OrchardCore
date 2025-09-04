namespace OrchardCore.Email;

public interface IEmailService
{
    /// <summary>
    /// Send the given message as email.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="providerName">The technical name of the Email provider. When null or empty, the default provider is used.</param>
    /// <returns>EmailResult object.</returns>
    Task<EmailResult> SendAsync(MailMessage message, string providerName = null);
}
