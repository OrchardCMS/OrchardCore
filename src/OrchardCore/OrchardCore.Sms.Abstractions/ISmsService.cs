using System.Threading.Tasks;

namespace OrchardCore.Sms;

public interface ISmsService
{
    /// <summary>
    /// Send the given message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="provider">An SMS Provider to use. When null, we sent using the default provider.</param>
    /// <returns>SmsResult object.</returns>
    Task<SmsResult> SendAsync(SmsMessage message, ISmsProvider provider = null);

}
