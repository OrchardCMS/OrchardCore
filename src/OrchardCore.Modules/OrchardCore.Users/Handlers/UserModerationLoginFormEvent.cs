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

internal sealed class UserModerationLoginFormEvent : LoginFormEventBase
{
    private readonly ISiteService _siteService;
    private readonly UserManager<IUser> _userManager;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserModerationLoginFormEvent(
        ISiteService siteService,
        UserManager<IUser> userManager,
        IHttpContextAccessor httpContextAccessor)
    {
        _siteService = siteService;
        _userManager = userManager;
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task<IActionResult> LoggingInAsync(IUser user)
    {
        var settings = await _siteService.GetSettingsAsync<RegistrationSettings>();

        if (!settings.UsersAreModerated || user is not User u || u.IsEnabled)
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
            actionName: nameof(RegistrationController.RegistrationPending),
            controllerName: typeof(RegistrationController).ControllerName(),
            routeValues: model);
    }
}
