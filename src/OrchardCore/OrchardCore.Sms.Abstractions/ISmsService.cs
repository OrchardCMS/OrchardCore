using OrchardCore.Infrastructure;

namespace OrchardCore.Sms;

public interface ISmsService
{
    /// <summary>
    /// Send the given message using the default provider.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the SMS was sent successfully.</returns>
    Task<Result> SendAsync(SmsMessage message, CancellationToken cancellationToken = default);
}
