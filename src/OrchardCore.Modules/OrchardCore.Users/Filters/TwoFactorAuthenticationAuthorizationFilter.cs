using Microsoft.AspNetCore.Http.Extensions;
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
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.HttpContext?.User?.Identity?.IsAuthenticated == false ||
            context.HttpContext.Request.Path.Equals("/" + _userOptions.LogoffPath, StringComparison.OrdinalIgnoreCase) ||
            context.HttpContext.Request.Path.Equals("/" + _userOptions.TwoFactorAuthenticationPath, StringComparison.OrdinalIgnoreCase) ||
            context.HttpContext.Request.Path.Equals("/TwoFactor-Authenticator/", StringComparison.OrdinalIgnoreCase))
        {
            await next();
            return;
        }

        var routeValues = context.HttpContext.Request.RouteValues;
        var areaName = routeValues["area"]?.ToString();

        if (areaName != null && string.Equals(areaName, UserConstants.Features.Users, StringComparison.OrdinalIgnoreCase))
        {
            var controllerName = routeValues["controller"]?.ToString();

            if (controllerName != null && _allowedControllerNames.Contains(controllerName, StringComparer.OrdinalIgnoreCase))
            {
                await next();
                return;
            }
        }

        if (context.HttpContext.User.HasClaim(claim => claim.Type == UserConstants.TwoFactorAuthenticationClaimType))
        {
            var user = await _userManager.GetUserAsync(context.HttpContext.User);

            if (user == null)
            {
                await next();
                return;
            }

            if (await _twoFactorHandlerCoordinator.IsRequiredAsync(user))
            {
                // 获取当前请求的 URL
                var originalUrl = context.HttpContext.Request.GetEncodedUrl();
                // 构建带有 returnUrl 参数的重定向 URL
                var redirectUrl = $"/{_userOptions.TwoFactorAuthenticationPath}?returnUrl={Uri.EscapeDataString(originalUrl)}";
                context.Result = new RedirectResult(redirectUrl);
                return;
            }
        }

        await next();
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
                // 获取当前访问的URL
                var currentUrl = context.HttpContext.Request.GetEncodedUrl();
                var requestScheme = context.HttpContext.Request.Scheme;
                var host = context.HttpContext.Request.Host;
// var currentUrl = $"{requestScheme}://{host}{context.HttpContext.Request.PathBase}{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}";

// var returnUrl = currentUrl.StartsWith($"{requestScheme}://{host}")
//     ? $"{context.HttpContext.Request.PathBase}{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}"
//     : currentUrl;
                // 如果是同域名，只保留路径部分
                var returnUrl = currentUrl.StartsWith($"{requestScheme}://{host}")
                    ? $"{context.HttpContext.Request.Path}{context.HttpContext.Request.QueryString}"
                    : currentUrl;

                // 生成重定向URL
                var redirectUrl = $"/{_userOptions.TwoFactorAuthenticationPath}?returnUrl={Uri.EscapeDataString(returnUrl)}";

                context.Result = new RedirectResult(redirectUrl);

             //   context.Result = new RedirectResult("~/" + _userOptions.TwoFactorAuthenticationPath);
            }
        }
    }
}
