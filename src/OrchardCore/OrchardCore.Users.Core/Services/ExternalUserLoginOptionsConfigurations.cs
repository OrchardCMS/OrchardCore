using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class ExternalUserLoginOptionsConfigurations : IConfigureOptions<ExternalUserLoginOptions>
{
    private readonly ISiteService _siteService;

    public ExternalUserLoginOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(ExternalUserLoginOptions options)
    {
        var settings = _siteService.GetSettingsAsync<ExternalUserLoginSettings>()
            .GetAwaiter()
            .GetResult();

        options.UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined;
    }
}
