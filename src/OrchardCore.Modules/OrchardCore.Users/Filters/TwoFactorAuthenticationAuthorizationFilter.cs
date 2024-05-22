using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.Filters;

public class TwoFactorAuthenticationAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly UserOptions _userOptions;
    private readonly UserManager<IUser> _userManager;
    private readonly ITwoFactorAuthenticationHandlerCoordinator _twoFactorHandlerCoordinator;
    private readonly AdminOptions _adminOptions;

    public TwoFactorAuthenticationAuthorizationFilter(
        IOptions<UserOptions> userOptions,
        IOptions<AdminOptions> adminOptions,
        UserManager<IUser> userManager,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorHandlerCoordinator)
    {
        _userOptions = userOptions.Value;
        _userManager = userManager;
        _twoFactorHandlerCoordinator = twoFactorHandlerCoordinator;
        _adminOptions = adminOptions.Value;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext?.User?.Identity?.IsAuthenticated == false
            || context.HttpContext.Request.Path.Equals("/" + _userOptions.LogoffPath, StringComparison.OrdinalIgnoreCase)
            || context.HttpContext.Request.Path.Equals("/" + _userOptions.TwoFactorAuthenticationPath, StringComparison.OrdinalIgnoreCase)
            || context.HttpContext.Request.Path.StartsWithSegments("/" + _adminOptions.AdminUrlPrefix + "/Authenticator/Configure", StringComparison.OrdinalIgnoreCase)
            || context.HttpContext.Request.Path.Equals("/" + _adminOptions.AdminUrlPrefix + "/EnableTwoFactorAuthentication", StringComparison.OrdinalIgnoreCase)
            || context.HttpContext.Request.Path.Equals("/" + _adminOptions.AdminUrlPrefix + "/ShowRecoveryCodes", StringComparison.OrdinalIgnoreCase)
            )
        {
            return;
        }

        if (context.HttpContext.User.HasClaim(claim => claim.Type == UserConstants.TwoFactorAuthenticationClaimType))
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            if (user == null)
            {
                return;
            }

            if (await _twoFactorHandlerCoordinator.IsRequiredAsync(user))
            {
                context.Result = new RedirectResult("~/" + _userOptions.TwoFactorAuthenticationPath);
            }
        }
    }
}
