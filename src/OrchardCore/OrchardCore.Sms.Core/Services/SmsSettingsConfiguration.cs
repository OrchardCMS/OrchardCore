using Microsoft.Extensions.Options;
using OrchardCore.Entities;
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
        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

        var settings = site.As<SmsSettings>();

        options.DefaultProviderName = settings.DefaultProviderName;
    }
}
