using System;
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

public class TwoFactorAuthenticationAuthorizationFilter : IAsyncAuthorizationFilter
{
    private readonly UserOptions _userOptions;

    public TwoFactorAuthenticationAuthorizationFilter(IOptions<UserOptions> userOptions)
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
            && (context.HttpContext?.User?.Identity?.IsAuthenticated ?? false))
        {
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<IUser>>();

            if (userManager != null)
            {
                var user = await userManager.GetUserAsync(context.HttpContext.User);

                if (!await userManager.GetTwoFactorEnabledAsync(user)
                    && await CanEnableTwoFactorAuthenticationAsync(settings, userManager, user))
                {
                    context.Result = new RedirectResult("~/EnableAuthenticator");
                }
            }
        }
    }

    private async Task<bool> CanEnableTwoFactorAuthenticationAsync(LoginSettings loginSettings, UserManager<IUser> userManager, IUser user)
    {
        if (loginSettings.EnableTwoFactorAuthenticationForSpecificRoles)
        {
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
