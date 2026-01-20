namespace OrchardCore.Sms;

public interface ISmsService
{
    /// <summary>
    /// Send the given message using the default provider.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>SmsResult object.</returns>
    Task<SmsResult> SendAsync(SmsMessage message);
}
