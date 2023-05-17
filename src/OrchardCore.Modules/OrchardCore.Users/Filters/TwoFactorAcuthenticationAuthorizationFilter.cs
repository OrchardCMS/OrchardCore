using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Entities;
using OrchardCore.Settings;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Filters;

public class TwoFactorAcuthenticationAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly UserOptions _userOptions;

    public TwoFactorAcuthenticationAuthorizationFilter(IOptions<UserOptions> userOptions)
    {
        _userOptions = userOptions.Value;
    }
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        if (context.HttpContext.Request.Path.Equals("/" + _userOptions.LogoffPath, StringComparison.OrdinalIgnoreCase)
            || context.HttpContext.Request.Path.Equals("/EnableAuthenticator", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var siteService = context.HttpContext.RequestServices.GetService<ISiteService>();

        if (siteService == null)
        {
            return;
        }
        var settings = (await siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (settings.RequireTwoFactorAuthentication
            && (context.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
            && !context.HttpContext.User.HasClaim(claim => claim.Type == "amr"))
        {
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<IUser>>();

            if (await CanEnableTwoFactorAuthenticationAsync(settings, userManager, context.HttpContext.User))
            {
                context.Result = new RedirectResult("~/EnableAuthenticator");
            }
        }
    }

    protected async Task<bool> CanEnableTwoFactorAuthenticationAsync(LoginSettings loginSettings, UserManager<IUser> userManager, ClaimsPrincipal principal)
    {
        if (loginSettings.EnableTwoFactorAuthenticationForSpecificRoles && userManager != null)
        {
            var user = await userManager.GetUserAsync(principal);

            foreach (var role in loginSettings.Roles)
            {
                if (await userManager.IsInRoleAsync(user, role))
                {
                    return true;
                }
            }

            return false;
        }

        return loginSettings.EnableTwoFactorAuthentication;
    }
}
