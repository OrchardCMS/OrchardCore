using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class RegistrationOptionsConfigurations : IConfigureOptions<RegistrationOptions>
{
    private readonly ISiteService _siteService;

    public RegistrationOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(RegistrationOptions options)
    {
        var settings = _siteService.GetSettingsAsync<RegistrationSettings>()
            .GetAwaiter()
            .GetResult();

        options.UsersMustValidateEmail = settings.UsersMustValidateEmail;
        options.UsersAreModerated = settings.UsersAreModerated;
        options.UseSiteTheme = settings.UseSiteTheme;
    }
}
