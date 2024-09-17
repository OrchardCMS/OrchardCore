using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class ExternalLoginOptionsConfigurations : IConfigureOptions<ExternalLoginOptions>
{
    private readonly ISiteService _siteService;

    public ExternalLoginOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(ExternalLoginOptions options)
    {
        var settings = _siteService.GetSettingsAsync<ExternalLoginSettings>()
            .GetAwaiter()
            .GetResult();

        options.UseExternalProviderIfOnlyOneDefined = settings.UseExternalProviderIfOnlyOneDefined;
    }
}
