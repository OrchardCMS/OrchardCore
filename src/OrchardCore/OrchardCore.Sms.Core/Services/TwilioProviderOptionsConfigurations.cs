using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;

namespace OrchardCore.Sms.Services;

public sealed class TwilioProviderOptionsConfigurations : IConfigureOptions<SmsProviderOptions>
{
    private readonly ISiteService _siteService;

    public TwilioProviderOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(SmsProviderOptions options)
    {
        var typeOptions = new SmsProviderTypeOptions(typeof(TwilioSmsProvider));

        var settings = _siteService.GetSettingsAsync<TwilioSettings>()
            .GetAwaiter()
            .GetResult();

        typeOptions.IsEnabled = settings.IsEnabled;

        options.TryAddProvider(TwilioSmsProvider.TechnicalName, typeOptions);
    }
}
