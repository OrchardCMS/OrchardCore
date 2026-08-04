using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class ExternalLoginFormEvents : LoginFormEventBase
{
    private readonly ExternalLoginOptions _externalLoginOptions;
    private readonly SignInManager<IUser> _signInManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ISiteService _siteService;

    public ExternalLoginFormEvents(
        IOptions<ExternalLoginOptions> externalLoginOptions,
        SignInManager<IUser> signInManager,
        LinkGenerator linkGenerator,
        IHttpContextAccessor httpContextAccessor,
        ISiteService siteService)
    {
        _externalLoginOptions = externalLoginOptions.Value;
        _signInManager = signInManager;
        _linkGenerator = linkGenerator;
        _httpContextAccessor = httpContextAccessor;
        _siteService = siteService;
    }

    public override async Task<IActionResult> LoggingInAsync()
    {
        if (!_externalLoginOptions.UseExternalProviderIfOnlyOneDefined)
        {
            return null;
        }

        // When the external provider returned a failure, the callback redirects back to /Login
        // with externalLoginError=true so the user can see the error instead of being challenged again.
        if (_httpContextAccessor.HttpContext.Request.Query.ContainsKey("externalLoginError"))
        {
            return null;
        }

        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

        if (schemes.Count() == 1)
        {
            var provider = schemes.First().Name;

            var model = new RouteValueDictionary();

            if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("returnUrl", out var returnUrlValue))
            {
                model.Add("returnUrl", returnUrlValue);
            }

            var redirectUrl = _linkGenerator.GetPathByAction(_httpContextAccessor.HttpContext,
                action: nameof(ExternalAuthenticationsController.ExternalLoginCallback),
                controller: typeof(ExternalAuthenticationsController).ControllerName(),
                values: model);

            var properties = _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            var loginSettings = await _siteService.GetSettingsAsync<LoginSettings>();

            properties.Items[nameof(LoginForm.RememberMe)] = loginSettings.UsePersistentAuthenticationCookie.ToString();

            return new ChallengeResult(
                authenticationScheme: provider,
                properties: properties);
        }

        return null;
    }
}
