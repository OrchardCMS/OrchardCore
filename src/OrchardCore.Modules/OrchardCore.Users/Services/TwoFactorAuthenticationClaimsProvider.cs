using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public class TwoFactorAuthenticationClaimsProvider : IUserClaimsProvider
{
    public const string TwoFactorAuthenticationClaimType = "TwoFacAuth";

    private readonly UserManager<IUser> _userManager;
    private readonly ISiteService _siteService;

    public TwoFactorAuthenticationClaimsProvider(
        UserManager<IUser> userManager,
        ISiteService siteService)
    {
        _userManager = userManager;
        _siteService = siteService;
    }

    public async Task GenerateAsync(IUser user, ClaimsIdentity claims)
    {
        if (user == null)
        {
            throw new ArgumentNullException(nameof(user));
        }

        if (claims == null)
        {
            throw new ArgumentNullException(nameof(claims));
        }

        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (loginSettings.RequireTwoFactorAuthentication
            && !await _userManager.GetTwoFactorEnabledAsync(user)
            && await loginSettings.CanEnableTwoFactorAuthenticationAsync(role => _userManager.IsInRoleAsync(user, role)))
        {
            // At this point, we know that the user must enable two-factor authentication.

            claims.AddClaim(new Claim(TwoFactorAuthenticationClaimType, "required"));
        }
    }
}
