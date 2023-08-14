using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;
using OrchardCore.Notifications;
using OrchardCore.Users.Models;

namespace OrchardCore.Sms.Services;

public class SmsNotificationProvider : INotificationMethodProvider
{
    private readonly ISmsProviderFactory _smsProviderFactory;
    protected readonly IStringLocalizer S;

    public SmsNotificationProvider(
        ISmsProviderFactory smsProviderFactory,
        IStringLocalizer<SmsNotificationProvider> stringLocalizer)
    {
        _smsProviderFactory = smsProviderFactory;
        S = stringLocalizer;
    }

    public string Method => "SMS";

    public LocalizedString Name => S["SMS Notifications"];

    public async Task<bool> TrySendAsync(object notify, INotificationMessage message)
    {
        var user = notify as User;

        if (String.IsNullOrEmpty(user?.Email))
        {
            return false;
        }

        var mailMessage = new SmsMessage()
        {
            To = user.Email,
            Body = message.TextBody,
        };

        var smsProvider = await _smsProviderFactory.CreateAsync();

        var result = await smsProvider.SendAsync(mailMessage);

        return result.Succeeded;
    }
}
