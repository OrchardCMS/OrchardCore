using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace OrchardCore.Sms.Services;

public class DefaultSmsProvider : ISmsProvider
{
    protected readonly IStringLocalizer S;

    public DefaultSmsProvider(IStringLocalizer<DefaultSmsProvider> stringLocalizer)
    {
        S = stringLocalizer;
    }

    public LocalizedString Name => S["Default"];

    public Task<SmsResult> SendAsync(SmsMessage message)
    {
        return Task.FromResult(SmsResult.Failed(S["SMS settings must be configured before an SMS message can be sent."]));
    }
}
