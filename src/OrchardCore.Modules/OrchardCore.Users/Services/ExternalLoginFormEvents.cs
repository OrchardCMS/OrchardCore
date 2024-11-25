using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class ExternalLoginFormEvents : ILoginFormEvent
{
    private const string ExternalLoginAutoRedirectCookieName = "ELAR";

    private readonly ExternalLoginOptions _externalLoginOptions;
    private readonly SignInManager<IUser> _signInManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly ShellSettings _shellSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExternalLoginFormEvents(
        IOptions<ExternalLoginOptions> externalLoginOptions,
        SignInManager<IUser> signInManager,
        LinkGenerator linkGenerator,
        ShellSettings shellSettings,
        IHttpContextAccessor httpContextAccessor)
    {
        _externalLoginOptions = externalLoginOptions.Value;
        _signInManager = signInManager;
        _linkGenerator = linkGenerator;
        _shellSettings = shellSettings;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task IsLockedOutAsync(IUser user)
        => Task.CompletedTask;

    public Task LoggedInAsync(IUser user)
        => Task.CompletedTask;

    public async Task<IActionResult> LoggingInAsync()
    {
        if (!_externalLoginOptions.UseExternalProviderIfOnlyOneDefined)
        {
            return null;
        }

        // To prevent infinite redirects, we store an ELAR cookie, which stands for ExternalLoginAutoRedirect.
        // This cookie helps us avoid executing the same redirect multiple times. 
        // The acronym ELAR has no special meaning, and any other value could be used in its place; it is chosen simply for brevity.
        if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue(ExternalLoginAutoRedirectCookieName, out var _))
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Delete(ExternalLoginAutoRedirectCookieName);

            return null;
        }

        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

        if (schemes.Count() == 1)
        {
            _httpContextAccessor.HttpContext.Response.Cookies.Append(ExternalLoginAutoRedirectCookieName, "1", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Path = !string.IsNullOrEmpty(_shellSettings.RequestUrlPrefix)
                ? _shellSettings.RequestUrlPrefix
                : "/",
            });

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

            return new ChallengeResult(
                authenticationScheme: provider,
                properties: _signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl));
        }

        return null;
    }

    public Task LoggingInAsync(string userName, Action<string, string> reportError)
        => Task.CompletedTask;

    public Task LoggingInFailedAsync(string userName)
        => Task.CompletedTask;

    public Task LoggingInFailedAsync(IUser user)
        => Task.CompletedTask;
}
