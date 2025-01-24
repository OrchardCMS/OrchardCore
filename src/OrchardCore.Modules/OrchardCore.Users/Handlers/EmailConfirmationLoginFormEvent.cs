using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

internal sealed class EmailConfirmationLoginFormEvent : LoginFormEventBase
{
    private readonly RegistrationOptions _registrationOptions;
    private readonly UserManager<IUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmailConfirmationLoginFormEvent(
        IOptions<RegistrationOptions> registrationOptions,
        UserManager<IUser> userManager,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _registrationOptions = registrationOptions.Value;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IActionResult> ValidatingLoginAsync(IUser user)
    {
        // Require that the users have a confirmed email before they can log on.
        if (!_registrationOptions.UsersMustValidateEmail || await _userManager.IsEmailConfirmedAsync(user))
        {
            return null;
        }

        var model = new RouteValueDictionary()
        {
            { "Area", UserConstants.Features.Users },
        };

        if (_httpContextAccessor.HttpContext.Request.Query.TryGetValue("returnUrl", out var returnUrlValue))
        {
            model.Add("returnUrl", returnUrlValue);
        }

        return new RedirectToActionResult(
            actionName: nameof(EmailConfirmationController.ConfirmEmailSent),
            controllerName: typeof(EmailConfirmationController).ControllerName(),
            routeValues: model);
    }
}
