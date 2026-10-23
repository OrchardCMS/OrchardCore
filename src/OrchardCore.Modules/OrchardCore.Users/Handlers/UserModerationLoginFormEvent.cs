using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

internal sealed class UserModerationLoginFormEvent : LoginFormEventBase
{
    private readonly IOptionsMonitor<RegistrationOptions> _registrationOptions;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserModerationLoginFormEvent(
        IOptionsMonitor<RegistrationOptions> registrationOptions,
        IHttpContextAccessor httpContextAccessor)
    {
        _registrationOptions = registrationOptions;
        _httpContextAccessor = httpContextAccessor;
    }

    public override Task<IActionResult> ValidatingLoginAsync(IUser user)
    {
        if (!_registrationOptions.CurrentValue.UsersAreModerated || user is not User u || u.IsEnabled)
        {
            return Task.FromResult<IActionResult>(null);
        }

        var model = new RouteValueDictionary()
        {
            { "Area", UserConstants.Features.Users },
        };

        if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("returnUrl", out var returnUrlValue))
        {
            model.Add("returnUrl", returnUrlValue);
        }

        return Task.FromResult<IActionResult>(new RedirectToActionResult(
            actionName: nameof(RegistrationController.RegistrationPending),
            controllerName: typeof(RegistrationController).ControllerName(),
            routeValues: model));
    }
}
