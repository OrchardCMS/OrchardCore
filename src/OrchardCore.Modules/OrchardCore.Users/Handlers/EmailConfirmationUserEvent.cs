using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Controllers;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Handlers;

internal sealed class EmailConfirmationUserEvent : LoginFormEventBase
{
    private readonly ISiteService _siteService;
    private readonly UserManager<IUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EmailConfirmationUserEvent(
        ISiteService siteService,
        UserManager<IUser> userManager,
        IHttpContextAccessor httpContextAccessor
        )
    {
        _siteService = siteService;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IActionResult> LoggingInAsync(IUser user)
    {
        var settings = await _siteService.GetSettingsAsync<LoginSettings>();

        // Require that the users have a confirmed email before they can log on.
        if (!settings.UsersMustValidateEmail || await _userManager.IsEmailConfirmedAsync(user))
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
