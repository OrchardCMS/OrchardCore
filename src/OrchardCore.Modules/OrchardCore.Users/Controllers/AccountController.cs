using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Authorize]
public sealed class AccountController : AccountBaseController
{
    private readonly IUserService _userService;
    private readonly SignInManager<IUser> _signInManager;
    private readonly UserManager<IUser> _userManager;
    private readonly ILogger _logger;
    private readonly ISiteService _siteService;
    private readonly IEnumerable<ILoginFormEvent> _accountEvents;
    private readonly RegistrationOptions _registrationOptions;
    private readonly IDisplayManager<LoginForm> _loginFormDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AccountController(
        IUserService userService,
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ILogger<AccountController> logger,
        ISiteService siteService,
        IHtmlLocalizer<AccountController> htmlLocalizer,
        IStringLocalizer<AccountController> stringLocalizer,
        IEnumerable<ILoginFormEvent> accountEvents,
        IOptions<RegistrationOptions> registrationOptions,
        INotifier notifier,
        IDisplayManager<LoginForm> loginFormDisplayManager,
        IUpdateModelAccessor updateModelAccessor)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userService = userService;
        _logger = logger;
        _siteService = siteService;
        _accountEvents = accountEvents;
        _registrationOptions = registrationOptions.Value;
        _notifier = notifier;
        _loginFormDisplayManager = loginFormDisplayManager;
        _updateModelAccessor = updateModelAccessor;

        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [AllowAnonymous]
    public async Task<IActionResult> Login(string returnUrl = null)
    {
        if (HttpContext.User?.Identity?.IsAuthenticated ?? false)
        {
            returnUrl = null;
        }

        // Clear the existing external cookie to ensure a clean login process.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        foreach (var accountEvent in _accountEvents)
        {
            var result = await accountEvent.LoggingInAsync();

            if (result != null)
            {
                return result;
            }
        }

        var formShape = await _loginFormDisplayManager.BuildEditorAsync(_updateModelAccessor.ModelUpdater, false);

        CopyTempDataErrorsToModelState();

        ViewData["ReturnUrl"] = returnUrl;

        return View(formShape);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(Login))]
    public async Task<IActionResult> LoginPOST(string returnUrl = null)
    {
        var loginSettings = await _siteService.GetSettingsAsync<LoginSettings>();

        if (loginSettings.DisableLocalLogin)
        {
            await _notifier.ErrorAsync(H["Local login is disabled."]);

            return RedirectToAction(nameof(Login), new { returnUrl });
        }

        ViewData["ReturnUrl"] = returnUrl;

        var model = new LoginForm();

        var formShape = await _loginFormDisplayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, false);

        await _accountEvents.InvokeAsync((e, model, modelState) => e.LoggingInAsync(model.UserName, (key, message) => modelState.AddModelError(key, message)), model, ModelState, _logger);

        IUser user = null;

        if (ModelState.IsValid)
        {
            user = await _userService.GetUserAsync(model.UserName);

            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    foreach (var accountEvent in _accountEvents)
                    {
                        var loginResult = await accountEvent.ValidatingLoginAsync(user);

                        if (loginResult != null)
                        {
                            return loginResult;
                        }
                    }

                    result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation(1, "User logged in.");

                        await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

                        return await LoggedInActionResultAsync(user, returnUrl);
                    }

                }

                if (result.RequiresTwoFactor)
                {
                    return RedirectToAction(
                        nameof(TwoFactorAuthenticationController.LoginWithTwoFactorAuthentication),
                        typeof(TwoFactorAuthenticationController).ControllerName(),
                        new
                        {
                            returnUrl,
                            model.RememberMe,
                        });
                }

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, S["The account is locked out"]);
                    await _accountEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

                    return View();
                }
            }

            ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
        }

        if (user == null)
        {
            // Login failed unknown user.
            await _accountEvents.InvokeAsync((e, model) => e.LoggingInFailedAsync(model.UserName), model, _logger);
        }
        else
        {
            // Login failed with a known user.
            await _accountEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);
        }

        // If we got this far, something failed, redisplay form.
        return View(formShape);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LogOff(string returnUrl = null)
    {
        await _signInManager.SignOutAsync();

        _logger.LogInformation(4, "User logged out.");

        return RedirectToLocal(returnUrl);
    }

    public IActionResult ChangePassword(string returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model, string returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            var user = await _userService.GetAuthenticatedUserAsync(User);

            if (await _userService.ChangePasswordAsync(user, model.CurrentPassword, model.Password, ModelState.AddModelError))
            {
                if (Url.IsLocalUrl(returnUrl))
                {
                    await _notifier.SuccessAsync(H["Your password has been changed successfully."]);

                    return this.Redirect(returnUrl, true);
                }

                return Redirect(Url.Action(nameof(ChangePasswordConfirmation)));
            }
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult ChangePasswordConfirmation()
        => View();
}
