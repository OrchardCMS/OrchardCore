using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public class DefaultTwoFactorAuthenticationHandler : ITwoFactorAuthenticationHandler
{
    private readonly ISiteService _siteService;

    public DefaultTwoFactorAuthenticationHandler(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public async Task<bool> IsRequiredAsync(IUser user)
        => (await _siteService.GetSettingsAsync<TwoFactorLoginSettings>()).RequireTwoFactorAuthentication;
}
