using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Controllers;

[Authorize]
[Feature(UserConstants.Features.UserSecurity)]
public sealed class SecurityController : Controller
{
    private readonly UserManager<IUser> _userManager;
    private readonly IDisplayManager<UserSecurity> _userSecurityDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    public SecurityController(
        UserManager<IUser> userManager,
        IDisplayManager<UserSecurity> userSecurityDisplayManager,
        IUpdateModelAccessor updateModelAccessor)
    {
        _userManager = userManager;
        _userSecurityDisplayManager = userSecurityDisplayManager;
        _updateModelAccessor = updateModelAccessor;
    }

    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return NotFound("Unable to load user.");
        }

        var shape = await _userSecurityDisplayManager.BuildDisplayAsync(new UserSecurity { User = user }, _updateModelAccessor.ModelUpdater);

        return View(shape);
    }
}
