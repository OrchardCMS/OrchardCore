using System.Security.Cryptography;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
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
    private const string ResendEmailConfirmationPurpose = "OrchardCore.Users.ResendEmailConfirmation";

    private readonly IUserService _userService;
    private readonly SignInManager<IUser> _signInManager;
    private readonly UserManager<IUser> _userManager;
    private readonly ILogger _logger;
    private readonly ISiteService _siteService;
    private readonly IEnumerable<ILoginFormEvent> _loginFormEvents;
    private readonly RegistrationOptions _registrationOptions;
    private readonly IDisplayManager<LoginForm> _loginFormDisplayManager;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly INotifier _notifier;
    private readonly UserEmailService _userEmailService;
    private readonly IDataProtector _resendEmailConfirmationProtector;

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
        IEnumerable<ILoginFormEvent> loginFormEvents,
        IOptions<RegistrationOptions> registrationOptions,
        INotifier notifier,
        UserEmailService userEmailService,
        IDataProtectionProvider dataProtectionProvider,
        IDisplayManager<LoginForm> loginFormDisplayManager,
        IUpdateModelAccessor updateModelAccessor)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _userService = userService;
        _logger = logger;
        _siteService = siteService;
        _loginFormEvents = loginFormEvents;
        _registrationOptions = registrationOptions.Value;
        _notifier = notifier;
        _userEmailService = userEmailService;
        _resendEmailConfirmationProtector = dataProtectionProvider.CreateProtector(ResendEmailConfirmationPurpose);
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

        foreach (var loginFormEvent in _loginFormEvents)
        {
            var result = await loginFormEvent.LoggingInAsync();

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

        await _loginFormEvents.InvokeAsync((e, model, modelState) => e.LoggingInAsync(model.UserName, (key, message) => modelState.AddModelError(key, message)), model, ModelState, _logger);

        IUser user = null;

        if (ModelState.IsValid)
        {
            user = await _userService.GetUserAsync(model.UserName);

            if (user != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    foreach (var loginFormEvent in _loginFormEvents)
                    {
                        var loginResult = await loginFormEvent.ValidatingLoginAsync(user);

                        if (loginResult != null)
                        {
                            if (IsConfirmEmailSentResult(loginResult) && await AddConfirmEmailErrorAsync(user))
                            {
                                return View(formShape);
                            }

                            return loginResult;
                        }
                    }

                    result = await _signInManager.PasswordSignInAsync(user, model.Password, model.RememberMe, lockoutOnFailure: true);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation(1, "User logged in.");

                        await _loginFormEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

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
                    await _loginFormEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

                    return View();
                }
            }

            ModelState.AddModelError(string.Empty, S["Invalid login attempt."]);
        }

        if (user == null)
        {
            // Login failed unknown user.
            await _loginFormEvents.InvokeAsync((e, model) => e.LoggingInFailedAsync(model.UserName), model, _logger);
        }
        else
        {
            // Login failed with a known user.
            await _loginFormEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);
        }

        // If we got this far, something failed, redisplay form.
        return View(formShape);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> ResendEmailConfirmation(string code, string returnUrl = null)
    {
        if (string.IsNullOrEmpty(code))
        {
            return NotFound();
        }

        var userId = string.Empty;

        try
        {
            userId = _resendEmailConfirmationProtector.Unprotect(code);
        }
        catch (CryptographicException)
        {
            return NotFound();
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user == null)
        {
            return NotFound();
        }

        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            await _userEmailService.SendEmailConfirmationAsync(user);
        }

        return RedirectToAction(
            nameof(EmailConfirmationController.ConfirmEmailSent),
            typeof(EmailConfirmationController).ControllerName(),
            new { returnUrl });
    }

    [HttpPost]
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

        ViewData["ReturnUrl"] = returnUrl;

        return View(model);
    }

    [HttpGet]
    public IActionResult ChangePasswordConfirmation()
        => View();

    private async Task<bool> AddConfirmEmailErrorAsync(IUser user)
    {
        if (!_registrationOptions.UsersMustValidateEmail || await _userManager.IsEmailConfirmedAsync(user))
        {
            return false;
        }

        ModelState.AddModelError(
            string.Empty,
            S["You must confirm your email. A confirmation email was sent when the account was created."]);

        ViewData["ResendEmailConfirmationCode"] = _resendEmailConfirmationProtector.Protect(
            await _userManager.GetUserIdAsync(user));

        return true;
    }

    private static bool IsConfirmEmailSentResult(IActionResult result)
        => result is RedirectToActionResult redirectToActionResult &&
            string.Equals(redirectToActionResult.ActionName, nameof(EmailConfirmationController.ConfirmEmailSent), StringComparison.Ordinal) &&
            string.Equals(redirectToActionResult.ControllerName, typeof(EmailConfirmationController).ControllerName(), StringComparison.Ordinal);
}
