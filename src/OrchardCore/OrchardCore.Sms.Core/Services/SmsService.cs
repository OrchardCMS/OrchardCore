using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;

namespace OrchardCore.Sms.Services;

public class SmsService : ISmsService
{
    private readonly SmsSettings _smsOptions;
    private readonly IServiceProvider _serviceProvider;

    protected readonly IStringLocalizer<SmsService> S;

    public SmsService(
        IServiceProvider serviceProvider,
        IOptions<SmsSettings> smsOptions,
        IStringLocalizer<SmsService> stringLocalizer)
    {
        _smsOptions = smsOptions.Value;
        _serviceProvider = serviceProvider;
        S = stringLocalizer;
    }

    public Task<SmsResult> SendAsync(SmsMessage message)
    {
        var provider = GetProvider();

        if (provider == null)
        {
            return Task.FromResult(SmsResult.Failed(S["SMS settings must be configured before an SMS message can be sent."]));
        }

        return provider.SendAsync(message);
    }

    private ISmsProvider GetProvider()
    {
        if (_provider == null && !string.IsNullOrEmpty(_smsOptions.DefaultProviderName))
        {
            _provider = _serviceProvider.GetRequiredKeyedService<ISmsProvider>(_smsOptions.DefaultProviderName);
        }

        return _provider;
    }

    private ISmsProvider _provider;
}
