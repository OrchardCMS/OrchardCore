using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.Services;

public class TwoFactorAuthenticationClaimsProvider : IUserClaimsProvider
{
    private readonly UserManager<IUser> _userManager;
    private readonly ITwoFactorAuthenticationHandlerCoordinator _twoFactorHandlerCoordinator;

    public TwoFactorAuthenticationClaimsProvider(
        UserManager<IUser> userManager,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorHandlerCoordinator)
    {
        _userManager = userManager;
        _twoFactorHandlerCoordinator = twoFactorHandlerCoordinator;
    }

    public async Task GenerateAsync(IUser user, ClaimsIdentity claims)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(claims);

        if (await _twoFactorHandlerCoordinator.IsRequiredAsync(user) &&
            !await _userManager.GetTwoFactorEnabledAsync(user))
        {
            // At this point, we know that the user must enable two-factor authentication.
            claims.AddClaim(new Claim(UserConstants.TwoFactorAuthenticationClaimType, "required"));
        }
    }
}
