using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;

namespace OrchardCore.Users.Filters;

public sealed class TwoFactorAuthenticationAuthorizationFilter : IAsyncAuthorizationFilter
{
    private static readonly string[] _allowedControllerNames =
    [
        typeof(EmailConfirmationController).ControllerName(),
        typeof(TwoFactorAuthenticationController).ControllerName(),
        typeof(AuthenticatorAppController).ControllerName(),
        typeof(SmsAuthenticatorController).ControllerName(),
    ];

    private readonly UserOptions _userOptions;
    private readonly UserManager<IUser> _userManager;
    private readonly ITwoFactorAuthenticationHandlerCoordinator _twoFactorHandlerCoordinator;

    public TwoFactorAuthenticationAuthorizationFilter(
        IOptions<UserOptions> userOptions,
        UserManager<IUser> userManager,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorHandlerCoordinator)
    {
        _userOptions = userOptions.Value;
        _userManager = userManager;
        _twoFactorHandlerCoordinator = twoFactorHandlerCoordinator;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext?.User?.Identity?.IsAuthenticated == false ||
            context.HttpContext.Request.Path.Equals("/" + _userOptions.LogoffPath, StringComparison.OrdinalIgnoreCase) ||
            context.HttpContext.Request.Path.Equals("/" + _userOptions.TwoFactorAuthenticationPath, StringComparison.OrdinalIgnoreCase) ||
            context.HttpContext.Request.Path.Equals("/TwoFactor-Authenticator/", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var routeValues = context.HttpContext.Request.RouteValues;
        var areaName = routeValues["area"]?.ToString();

        if (areaName != null && string.Equals(areaName, UserConstants.Features.Users, StringComparison.OrdinalIgnoreCase))
        {
            var controllerName = routeValues["controller"]?.ToString();

            if (controllerName != null && _allowedControllerNames.Contains(controllerName, StringComparer.OrdinalIgnoreCase))
            {
                return;
            }
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
