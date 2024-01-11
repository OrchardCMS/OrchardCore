using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.Settings;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.Filters;

public class TwoFactorAuthenticationAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly UserOptions _userOptions;
    private readonly ITwoFactorAuthenticationHandlerCoordinator _twoFactorHandlerCoordinator;
    private readonly AdminOptions _adminOptions;

    private ISiteService _siteService;

    public TwoFactorAuthenticationAuthorizationFilter(
        IOptions<UserOptions> userOptions,
        IOptions<AdminOptions> adminOptions,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorHandlerCoordinator)
    {
        _userOptions = userOptions.Value;
        _twoFactorHandlerCoordinator = twoFactorHandlerCoordinator;
        _adminOptions = adminOptions.Value;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (!(context.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
            || context.HttpContext.Request.Path.Equals("/" + _userOptions.LogoffPath, StringComparison.OrdinalIgnoreCase)
            || context.HttpContext.Request.Path.Equals("/" + _userOptions.TwoFactorAuthenticationPath, StringComparison.OrdinalIgnoreCase)
            || context.HttpContext.Request.Path.StartsWithSegments("/" + _adminOptions.AdminUrlPrefix + "/Authenticator/Configure", StringComparison.OrdinalIgnoreCase)
            || context.HttpContext.Request.Path.Equals("/" + _adminOptions.AdminUrlPrefix + "/ShowRecoveryCodes", StringComparison.OrdinalIgnoreCase)
            )
        {
            return;
        }

        _siteService ??= context.HttpContext.RequestServices.GetService<ISiteService>();

        if (_siteService == null)
        {
            return;
        }

        if (await _twoFactorHandlerCoordinator.IsRequiredAsync()
            && context.HttpContext.User.HasClaim(claim => claim.Type == UserConstants.TwoFactorAuthenticationClaimType))
        {
            context.Result = new RedirectResult("~/" + _userOptions.TwoFactorAuthenticationPath);
        }
    }
}
