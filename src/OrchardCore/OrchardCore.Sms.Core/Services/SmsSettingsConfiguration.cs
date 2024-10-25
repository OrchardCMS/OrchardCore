using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Sms.Services;

public class SmsSettingsConfiguration : IPostConfigureOptions<SmsSettings>
{
    private readonly ISiteService _siteService;

    public SmsSettingsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void PostConfigure(string name, SmsSettings options)
    {
        var settings = _siteService.GetSettingsAsync<SmsSettings>()
            .GetAwaiter()
            .GetResult();

        options.DefaultProviderName = settings.DefaultProviderName;
    }
}
