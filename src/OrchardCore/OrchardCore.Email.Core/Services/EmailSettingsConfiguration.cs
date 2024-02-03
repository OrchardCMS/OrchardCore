using Microsoft.Extensions.Options;
using OrchardCore.Settings;

namespace OrchardCore.Email.Core.Services;

public class EmailSettingsConfiguration : IPostConfigureOptions<EmailSettings>
{
    private readonly ISiteService _siteService;

    public EmailSettingsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void PostConfigure(string name, EmailSettings options)
    {
        var site = _siteService.GetSiteSettingsAsync().GetAwaiter().GetResult();

        var settings = site.As<EmailSettings>();

        options.DefaultProviderName = settings.DefaultProviderName;
    }
}
