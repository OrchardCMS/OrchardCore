using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.Admin;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;

namespace OrchardCore.Users.Controllers;

[Admin]
[Feature(UserConstants.Features.TwoFactorAuthentication)]
public sealed class AdminTwoFactorAuthenticationController : Controller
{
    private readonly UserManager<IUser> _userManager;
    private readonly IAuthorizationService _authorizationService;

    public AdminTwoFactorAuthenticationController(
        UserManager<IUser> userManager,
        IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
    }

    public async Task<IActionResult> Disable(string id)
    {
        if (!await _authorizationService.AuthorizeAsync(User, CommonPermissions.DisableTwoFactorAuthenticationForUsers))
        {
            return Forbid();
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }



        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            await _userManager.SetTwoFactorEnabledAsync(user, false);
        }

        return RedirectToAction(nameof(AdminController.Index), typeof(AdminController).ControllerName());
    }
}
