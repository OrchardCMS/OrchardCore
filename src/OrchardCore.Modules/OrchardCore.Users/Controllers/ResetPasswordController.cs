using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Feature(UserConstants.Features.ResetPassword)]
public sealed class ResetPasswordController : Controller
{
    private static readonly string _controllerName = typeof(ResetPasswordController).ControllerName();

    private readonly IUserService _userService;
    private readonly UserManager<IUser> _userManager;
    private readonly ISiteService _siteService;
    private readonly IEnumerable<IPasswordRecoveryFormEvents> _passwordRecoveryFormEvents;
    private readonly ILogger _logger;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IDisplayManager<ForgotPasswordForm> _forgotPasswordDisplayManager;
    private readonly IDisplayManager<ResetPasswordForm> _resetPasswordDisplayManager;
    private readonly IShellFeaturesManager _shellFeaturesManager;
    private readonly RegistrationOptions _registrationOptions;

    internal readonly IStringLocalizer S;

    public ResetPasswordController(
        IUserService userService,
        UserManager<IUser> userManager,
        ISiteService siteService,
        ILogger<ResetPasswordController> logger,
        IUpdateModelAccessor updateModelAccessor,
        IDisplayManager<ForgotPasswordForm> forgotPasswordDisplayManager,
        IDisplayManager<ResetPasswordForm> resetPasswordDisplayManager,
        IShellFeaturesManager shellFeaturesManager,
        IOptions<RegistrationOptions> registrationOptions,
        IEnumerable<IPasswordRecoveryFormEvents> passwordRecoveryFormEvents,
        IStringLocalizer<ResetPasswordController> stringLocalizer)
    {
        _userService = userService;
        _userManager = userManager;
        _siteService = siteService;
        _logger = logger;
        _updateModelAccessor = updateModelAccessor;
        _forgotPasswordDisplayManager = forgotPasswordDisplayManager;
        _resetPasswordDisplayManager = resetPasswordDisplayManager;
        _shellFeaturesManager = shellFeaturesManager;
        _registrationOptions = registrationOptions.Value;
        _passwordRecoveryFormEvents = passwordRecoveryFormEvents;
        S = stringLocalizer;
    }

    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword()
    {
        if (!(await _siteService.GetSettingsAsync<ResetPasswordSettings>()).AllowResetPassword)
        {
            return NotFound();
        }

        var formShape = await _forgotPasswordDisplayManager.BuildEditorAsync(_updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        return View(formShape);
    }

    [HttpPost]
    [AllowAnonymous]
    [ActionName(nameof(ForgotPassword))]
    public async Task<IActionResult> ForgotPasswordPOST()
    {
        if (!(await _siteService.GetSettingsAsync<ResetPasswordSettings>()).AllowResetPassword)
        {
            return NotFound();
        }

        var model = new ForgotPasswordForm();

        var formShape = await _forgotPasswordDisplayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        await _passwordRecoveryFormEvents.InvokeAsync((e, modelState) => e.RecoveringPasswordAsync((key, message) => modelState.AddModelError(key, message)), ModelState, _logger);

        if (ModelState.IsValid)
        {
            var user = await _userService.GetForgotPasswordUserAsync(model.UsernameOrEmail) as User;
            if (user == null || await MustValidateEmailAsync(user))
            {
                // returns to confirmation page anyway: we don't want to let scrapers know if a username or an email exist
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            user.ResetToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(user.ResetToken));
            var resetPasswordUrl = Url.Action(nameof(ResetPassword), _controllerName, new { code = user.ResetToken }, HttpContext.Request.Scheme);

            // send email with callback link
            await this.SendEmailAsync(user.Email, S["Reset your password"], new LostPasswordViewModel()
            {
                User = user,
                LostPasswordUrl = resetPasswordUrl
            });

            var context = new PasswordRecoveryContext(user);

            await _passwordRecoveryFormEvents.InvokeAsync((handler, context) => handler.PasswordRecoveredAsync(context), context, _logger);

            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        // If we got this far, something failed, redisplay form
        return View(formShape);
    }

    [AllowAnonymous]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword(string code = null)
    {
        if (!(await _siteService.GetSettingsAsync<ResetPasswordSettings>()).AllowResetPassword)
        {
            return NotFound();
        }

        if (string.IsNullOrWhiteSpace(code))
        {
            return RedirectToAction(nameof(ForgotPassword));
        }

        var model = new ResetPasswordForm { ResetToken = code };
        var shape = await _resetPasswordDisplayManager.BuildEditorAsync(model, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        return View(shape);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [ActionName(nameof(ResetPassword))]
    public async Task<IActionResult> ResetPasswordPOST()
    {
        if (!(await _siteService.GetSettingsAsync<ResetPasswordSettings>()).AllowResetPassword)
        {
            return NotFound();
        }

        var model = new ResetPasswordForm();
        var shape = await _resetPasswordDisplayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        await _passwordRecoveryFormEvents.InvokeAsync((e, modelState) => e.ResettingPasswordAsync((key, message) => modelState.AddModelError(key, message)), ModelState, _logger);

        if (ModelState.IsValid)
        {
            var token = Base64.FromUTF8Base64String(model.ResetToken);

            if (await _userService.ResetPasswordAsync(model.UsernameOrEmail, token, model.NewPassword, ModelState.AddModelError))
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }
        }

        return View(shape);
    }

    [AllowAnonymous]
    public IActionResult ResetPasswordConfirmation()
    {
        return View();
    }

    private async Task<bool> MustValidateEmailAsync(User user)
    {
        return _registrationOptions.UsersMustValidateEmail &&
            !await _userManager.IsEmailConfirmedAsync(user);
    }
}
