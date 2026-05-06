using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Localization;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

internal sealed class DisabledUserLoginFormEvent : LoginFormEventBase
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITempDataDictionaryFactory _tempDataDictionaryFactory;

    private readonly IStringLocalizer S;

    public DisabledUserLoginFormEvent(
        IHttpContextAccessor httpContextAccessor,
        ITempDataDictionaryFactory tempDataDictionaryFactory,
        IStringLocalizer<DisabledUserLoginFormEvent> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _tempDataDictionaryFactory = tempDataDictionaryFactory;
        S = stringLocalizer;
    }

    public override async Task<IActionResult> ValidatingLoginAsync(IUser user)
    {
        // Allow the login when the user account is enabled. The default User implementation
        // exposes IsEnabled; non-User implementations are not subject to this check.
        if (user is not User localUser || localUser.IsEnabled)
        {
            return null;
        }

        var httpContext = _httpContextAccessor.HttpContext;

        // If the user reached this point through an external provider, the external authentication
        // cookie has already been set. Sign it out so the disabled user is not left with a partial
        // authentication state that other handlers could pick up.
        await httpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        // Surface the error on the next request through the same TempData "error_*" channel that
        // AccountBaseController.CopyTempDataErrorsToModelState reads on the login GET.
        var tempData = _tempDataDictionaryFactory.GetTempData(httpContext);
        tempData["error_disabled_user"] = S["The specified user is not allowed to sign in."].Value;

        var routeValues = new RouteValueDictionary();

        if (httpContext.Request.Query.TryGetValue("returnUrl", out var returnUrlValue))
        {
            routeValues.Add("returnUrl", returnUrlValue);
        }

        return new RedirectToActionResult(
            actionName: nameof(AccountController.Login),
            controllerName: typeof(AccountController).ControllerName(),
            routeValues: routeValues);
    }
}
