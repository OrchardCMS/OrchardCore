using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

internal sealed class DefaultExternalAuthenticationsStartupFilter : IStartupFilter
{
    private readonly UserOptions _userOptions;
    private readonly ExternalLoginOptions _externalLoginOptions;

    public DefaultExternalAuthenticationsStartupFilter(
        IOptions<UserOptions> userOptions,
        IOptions<ExternalLoginOptions> externalLoginOptions)
    {
        _userOptions = userOptions.Value;
        _externalLoginOptions = externalLoginOptions.Value;
    }

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return builder =>
        {
            builder.Use(async (context, next) =>
            {
                if (_externalLoginOptions.UseExternalProviderIfOnlyOneDefined &&
                // !context.Items.ContainsKey("DefaultExternalLogin") &&
                context.Request.Method == HttpMethods.Get &&
                (context.Request.Path.StartsWithSegments('/' + _userOptions.LoginPath, StringComparison.OrdinalIgnoreCase) ||
                context.Request.Path.StartsWithSegments("/OrchardCore.Users/ExternalAuthentications/DefaultExternalLogin", StringComparison.OrdinalIgnoreCase)))
                {
                    // Clear the existing external cookie to ensure a clean login process.
                    await context.SignOutAsync(IdentityConstants.ExternalScheme);

                    var signInManager = context.RequestServices.GetRequiredService<SignInManager<IUser>>();

                    var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();

                    if (schemes.Count() == 1)
                    {
                        var provider = schemes.First().Name;

                        context.Request.Path = "/OrchardCore.Users/ExternalAuthentications/ExternalLoginCallback";
                        context.Request.RouteValues["controller"] = typeof(ExternalAuthenticationsController).ControllerName();
                        context.Request.RouteValues["action"] = nameof(ExternalAuthenticationsController.ExternalLoginCallback);
                        context.Request.RouteValues["area"] = "OrchardCore.Users";

                        context.Items["DefaultExternalLogin"] = true;

                        await context.ChallengeAsync(
                            scheme: provider,
                            properties: signInManager.ConfigureExternalAuthenticationProperties(provider, GetReturnUrl(context)));

                        return;
                    }
                }

                await next();
            });

            next(builder);
        };
    }

    private static string GetReturnUrl(HttpContext context)
    {
        if (!(context.User?.Identity?.IsAuthenticated ?? false) &&
            context.Request.Query.TryGetValue("returnUrl", out var returnUrlValue))
        {
            return returnUrlValue;
        }

        return string.Empty;
    }
}
