using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

internal sealed class DefaultExternalAuthenticationsMiddleware
{
    private readonly RequestDelegate _next;

    private readonly UserOptions _userOptions;
    private readonly ExternalLoginOptions _externalLoginOptions;

    public DefaultExternalAuthenticationsMiddleware(
        RequestDelegate next,
        IOptions<UserOptions> userOptions,
        IOptions<ExternalLoginOptions> externalLoginOptions)
    {
        _userOptions = userOptions.Value;
        _externalLoginOptions = externalLoginOptions.Value;
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Response.HasStarted)
        {
            await _next(context);

            return;
        }

        if (_externalLoginOptions.UseExternalProviderIfOnlyOneDefined &&
        context.Request.Method == HttpMethods.Get &&
        IsLoginRequest(context))
        {
            // Clear the existing external cookie to ensure a clean login process.
            await context.SignOutAsync(IdentityConstants.ExternalScheme);

            var signInManager = context.RequestServices.GetRequiredService<SignInManager<IUser>>();
            var linkGenerator = context.RequestServices.GetRequiredService<LinkGenerator>();

            var schemes = await signInManager.GetExternalAuthenticationSchemesAsync();

            if (schemes.Count() == 1)
            {
                var provider = schemes.First().Name;

                var model = new RouteValueDictionary();

                if (context.Request.Query.TryGetValue("returnUrl", out var returnUrlValue))
                {
                    model.Add("returnUrl", returnUrlValue);
                }

                var redirectUrl = linkGenerator.GetPathByAction(context,
                    action: nameof(ExternalAuthenticationsController.ExternalLoginCallback),
                    controller: typeof(ExternalAuthenticationsController).ControllerName(),
                    values: model);

                await context.ChallengeAsync(
                    scheme: provider,
                    properties: signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl));

                return;
            }
        }

        await _next(context);
    }

    private bool IsLoginRequest(HttpContext context)
    {
        // /OrchardCore.Users/ExternalAuthentications/DefaultExternalLogin is used for backward compatibility.
        // This can be removed in v3.
        return context.Request.Path.StartsWithSegments('/' + _userOptions.LoginPath, StringComparison.OrdinalIgnoreCase) ||
            context.Request.Path.StartsWithSegments("/OrchardCore.Users/ExternalAuthentications/DefaultExternalLogin", StringComparison.OrdinalIgnoreCase);
    }
}
