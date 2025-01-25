using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Services;

public sealed class ExternalLoginFormEvents : LoginFormEventBase
{
    private const string ExternalLoginAutoRedirectKeyName = "ELAR";

    private readonly ExternalLoginOptions _externalLoginOptions;
    private readonly SignInManager<IUser> _signInManager;
    private readonly LinkGenerator _linkGenerator;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ExternalLoginFormEvents(
        IOptions<ExternalLoginOptions> externalLoginOptions,
        SignInManager<IUser> signInManager,
        LinkGenerator linkGenerator,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IHttpContextAccessor httpContextAccessor)
    {
        _externalLoginOptions = externalLoginOptions.Value;
        _signInManager = signInManager;
        _linkGenerator = linkGenerator;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task LoggedInAsync(IUser user)
    {
        var tempData = _tempDataDictionaryFactory.GetTempData(_httpContextAccessor.HttpContext);
        tempData.Remove(ExternalLoginAutoRedirectKeyName);

        return Task.CompletedTask;
    }

    public override async Task<IActionResult> LoggingInAsync()
    {
        if (!_externalLoginOptions.UseExternalProviderIfOnlyOneDefined)
        {
            return null;
        }

        var tempData = _tempDataDictionaryFactory.GetTempData(_httpContextAccessor.HttpContext);

        // To prevent infinite redirects, we add temp data.
        if (tempData.ContainsKey(ExternalLoginAutoRedirectKeyName))
        {
            return null;
        }

        var schemes = await _signInManager.GetExternalAuthenticationSchemesAsync();

        if (schemes.Count() == 1)
        {
            var provider = schemes.First().Name;

            tempData.Add(ExternalLoginAutoRedirectKeyName, true);

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
}
