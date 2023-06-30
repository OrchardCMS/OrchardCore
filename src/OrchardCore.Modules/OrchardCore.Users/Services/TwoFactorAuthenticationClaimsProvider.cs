using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Settings;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.Services;

public class TwoFactorAuthenticationClaimsProvider : IUserClaimsProvider
{
    public const string TwoFactorAuthenticationClaimType = "TwoFacAuth";

    private readonly UserManager<IUser> _userManager;
    private readonly ISiteService _siteService;
    private readonly ITwoFactorAuthenticationHandlerCoordinator _twoFactorHandlerCoordinator;

    public TwoFactorAuthenticationClaimsProvider(
        UserManager<IUser> userManager,
        ISiteService siteService,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorHandlerCoordinator)
    {
        _userManager = userManager;
        _siteService = siteService;
        _twoFactorHandlerCoordinator = twoFactorHandlerCoordinator;
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

        if (await _twoFactorHandlerCoordinator.IsRequiredAsync()
            && !await _userManager.GetTwoFactorEnabledAsync(user))
        {
            // At this point, we know that the user must enable two-factor authentication.
            claims.AddClaim(new Claim(TwoFactorAuthenticationClaimType, "required"));
        }
    }
}
