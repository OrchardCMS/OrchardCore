using Microsoft.Extensions.Localization;

namespace OrchardCore.Sms.Services;

public class SmsService : ISmsService
{
    private readonly ISmsProviderResolver _smsProviderResolver;
    private ISmsProvider _provider;

    protected readonly IStringLocalizer S;

    public SmsService(
        ISmsProviderResolver smsProviderResolver,
        IStringLocalizer<SmsService> stringLocalizer)
    {
        _smsProviderResolver = smsProviderResolver;
        S = stringLocalizer;
    }

    public async Task<SmsResult> SendAsync(SmsMessage message)
    {
        _provider ??= await _smsProviderResolver.GetAsync();

        if (_provider is null)
        {
            return SmsResult.Failed(S["SMS settings must be configured before an SMS message can be sent."]);
        }

        return await _provider.SendAsync(message);
    }
}
