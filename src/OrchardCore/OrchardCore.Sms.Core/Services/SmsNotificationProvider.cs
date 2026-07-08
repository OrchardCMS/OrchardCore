using Microsoft.Extensions.Localization;
using OrchardCore.Infrastructure;
using OrchardCore.Notifications;
using OrchardCore.Users.Models;

namespace OrchardCore.Sms.Services;

public class SmsNotificationProvider : INotificationMethodProvider
{
    private readonly ISmsService _smsService;
    protected readonly IStringLocalizer S;

    public SmsNotificationProvider(
        ISmsService smsService,
        IStringLocalizer<SmsNotificationProvider> stringLocalizer)
    {
        _smsService = smsService;
        S = stringLocalizer;
    }

    public string Method { get; } = "SMS";

    public LocalizedString Name => S["SMS Notifications"];

    /// <summary>
    /// Attempts to send the specified notification message to the recipient through SMS.
    /// </summary>
    /// <param name="notify">The recipient or notifiable object.</param>
    /// <param name="message">The notification message to send.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Result"/> describing whether the SMS notification was sent successfully.</returns>
    public async Task<Result> SendAsync(object notify, INotificationMessage message, CancellationToken cancellationToken = default)
    {
        var user = notify as User;

        if (string.IsNullOrEmpty(user?.PhoneNumber))
        {
            return Result.Failed(S["No phone number provided."]);
        }

        return await _smsService.SendAsync(user.PhoneNumber, message.TextBody, cancellationToken);
    }
}
