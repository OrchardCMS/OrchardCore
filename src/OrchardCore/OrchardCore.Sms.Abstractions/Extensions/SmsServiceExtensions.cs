using OrchardCore.Infrastructure;

namespace OrchardCore.Sms;

/// <summary>
/// Provides an extension methods for <see cref="ISmsService"/>.
/// </summary>
public static class SmsServiceExtensions
{
    /// <summary>
    /// Sends the specified message to an SMS server for delivery.
    /// </summary>
    /// <param name="smsService">The <see cref="ISmsService"/>.</param>
    /// <param name="to">The phone number to send the message to.</param>
    /// <param name="body">The body of the message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the SMS was sent successfully.</returns>
    public static Task<Result> SendAsync(this ISmsService smsService, string to, string body, CancellationToken cancellationToken = default)
    {
        var message = new SmsMessage
        {
            To = to,
            Body = body,
        };

        return smsService.SendAsync(message, cancellationToken);
    }
}
