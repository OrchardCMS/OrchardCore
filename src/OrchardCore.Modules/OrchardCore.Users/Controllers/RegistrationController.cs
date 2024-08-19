using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Handlers;
using OrchardCore.Users.Models;

namespace OrchardCore.Users.Controllers;

[Feature(UserConstants.Features.UserRegistration)]
public class RegistrationController : Controller
{
    private readonly UserManager<IUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISiteService _siteService;
    private readonly INotifier _notifier;
    private readonly ILogger _logger;
    private readonly IDisplayManager<RegisterUserForm> _registerUserDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;

    protected readonly IStringLocalizer S;
    private readonly IEnumerable<IUserEventHandler> _userEventHandlers;
    protected readonly IHtmlLocalizer H;

    public RegistrationController(
        UserManager<IUser> userManager,
        IAuthorizationService authorizationService,
        ISiteService siteService,
        INotifier notifier,
        ILogger<RegistrationController> logger,
        IDisplayManager<RegisterUserForm> registerUserDisplayManager,
        IUpdateModelAccessor updateModelAccessor,
        IHtmlLocalizer<RegistrationController> htmlLocalizer,
        IStringLocalizer<RegistrationController> stringLocalizer,
        IEnumerable<IUserEventHandler> userEventHandlers)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
        _siteService = siteService;
        _notifier = notifier;
        _logger = logger;
        _registerUserDisplayManager = registerUserDisplayManager;
        _updateModelAccessor = updateModelAccessor;
        H = htmlLocalizer;
        S = stringLocalizer;
        _userEventHandlers = userEventHandlers;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Register(string returnUrl = null)
    {
        var settings = await _siteService.GetSettingsAsync<RegistrationSettings>();
        if (settings.UsersCanRegister != UserRegistrationType.AllowRegistration)
        {
            return NotFound();
        }

        var shape = await _registerUserDisplayManager.BuildEditorAsync(_updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        ViewData["ReturnUrl"] = returnUrl;

        return View(shape);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Register))]
    public async Task<IActionResult> RegisterPOST(string returnUrl = null)
    {
        var settings = await _siteService.GetSettingsAsync<RegistrationSettings>();

        if (settings.UsersCanRegister != UserRegistrationType.AllowRegistration)
        {
            return NotFound();
        }

        var model = new RegisterUserForm();

        var shape = await _registerUserDisplayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var iUser = await this.RegisterUser(model, S["Confirm your account"], _logger);

            // If we get a user, redirect to returnUrl
            if (iUser is User user)
            {
                if (settings.UsersMustValidateEmail && !user.EmailConfirmed)
                {
                    return RedirectToAction(nameof(EmailConfirmationController.ConfirmEmailSent), typeof(EmailConfirmationController).ControllerName(), new { ReturnUrl = returnUrl });
                }

                if (settings.UsersAreModerated && !user.IsEnabled)
                {
                    return RedirectToAction(nameof(RegistrationPending), new { ReturnUrl = returnUrl });
                }

                return RedirectToLocal(returnUrl.ToUriComponents());
            }
        }

        // If we got this far, something failed. Let's redisplay form.
        return View(shape);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ConfirmEmail(string userId, string code)
    {
        if (userId == null || code == null)
        {
            return RedirectToAction(nameof(Register));
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        var result = await _userManager.ConfirmEmailAsync(user, code);

        if (result.Succeeded)
        {
            var userContext = new UserContext(user);
            await _userEventHandlers.InvokeAsync((handler, context) => handler.ConfirmedAsync(userContext), userContext, _logger);

            return View();
        }

        return NotFound();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RegistrationPending(string returnUrl = null)
        => View(new { ReturnUrl = returnUrl });

    private RedirectResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return Redirect("~/");
    }
}
