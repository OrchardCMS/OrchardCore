using System.Threading.Tasks;

namespace OrchardCore.Sms;

public interface ISmsProvider
{
    /// <summary>
    /// The name of the provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Send the given message.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <returns>SmsResult object</returns>
    Task<SmsResult> SendAsync(SmsMessage message);
}
