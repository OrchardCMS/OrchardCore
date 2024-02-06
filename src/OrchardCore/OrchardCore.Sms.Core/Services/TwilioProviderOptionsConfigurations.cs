using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;

namespace OrchardCore.Sms.Services;

public class TwilioProviderOptionsConfigurations : IConfigureOptions<SmsProviderOptions>
{
    private readonly ISiteService _siteService;

    public TwilioProviderOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(SmsProviderOptions options)
    {
        var typeOptions = new SmsProviderTypeOptions(typeof(TwilioSmsProvider));

        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();
        var settings = site.As<TwilioSettings>();

        typeOptions.IsEnabled = settings.IsEnabled;

        options.TryAddProvider(TwilioSmsProvider.TechnicalName, typeOptions);
    }
}
