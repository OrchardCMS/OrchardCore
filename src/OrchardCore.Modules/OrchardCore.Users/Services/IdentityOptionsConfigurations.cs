using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class IdentityOptionsConfigurations : IConfigureOptions<IdentityOptions>
{
    private readonly ISiteService _siteService;

    public IdentityOptionsConfigurations(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(IdentityOptions options)
    {
        var settings = _siteService.GetSettingsAsync<IdentitySettings>()
            .GetAwaiter()
            .GetResult();

        if (!string.IsNullOrEmpty(settings.UserSettings?.AllowedUserNameCharacters))
        {
            options.User.AllowedUserNameCharacters = settings.UserSettings.AllowedUserNameCharacters;
        }
        else
        {
            options.User.AllowedUserNameCharacters = IdentityUserSettings.DefaultAllowedUserNameCharacters;
        }

        // By default, we require a unique email for every user.
        options.User.RequireUniqueEmail = settings.UserSettings?.RequireUniqueEmail ?? true;
    }
}
