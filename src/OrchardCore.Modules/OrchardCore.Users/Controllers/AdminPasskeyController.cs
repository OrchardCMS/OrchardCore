using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Controllers;

[Admin]
[Feature(UserConstants.Features.Passkeys)]
public sealed class AdminPasskeyController : Controller
{
    private readonly UserManager<IUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly INotifier _notifier;
    private readonly TwoFactorOptions _twoFactorOptions;

    internal readonly IHtmlLocalizer H;

    public AdminPasskeyController(
        UserManager<IUser> userManager,
        IAuthorizationService authorizationService,
        INotifier notifier,
        IOptions<TwoFactorOptions> twoFactorOptions,
        IHtmlLocalizer<AdminPasskeyController> htmlLocalizer)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
        _notifier = notifier;
        _twoFactorOptions = twoFactorOptions.Value;
        H = htmlLocalizer;
    }

    [HttpPost]
    public async Task<IActionResult> RemoveAll(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        if (!await _authorizationService.AuthorizeAsync(User, UsersPermissions.EditUsers, user))
        {
            return Forbid();
        }

        foreach (var passkey in await _userManager.GetPasskeysAsync(user))
        {
            await _userManager.RemovePasskeyAsync(user, passkey.CredentialId);
        }

        // Don't leave two-factor authentication enabled without any method to complete it.
        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            var providers = await _userManager.GetValidTwoFactorProvidersAsync(user);

            if (!providers.Intersect(_twoFactorOptions.Providers).Any())
            {
                await _userManager.SetTwoFactorEnabledAsync(user, false);
            }
        }

        await _notifier.SuccessAsync(H["The passkeys of the user were removed."]);

        return RedirectToAction(nameof(AdminController.Index), typeof(AdminController).ControllerName());
    }
}
