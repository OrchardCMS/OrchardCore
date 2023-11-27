using System.Threading.Tasks;

namespace OrchardCore.Sms;

public interface ISmsService
{
    /// <summary>
    /// Send the given message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>SmsResult object.</returns>
    Task<SmsResult> SendAsync(SmsMessage message);
}
