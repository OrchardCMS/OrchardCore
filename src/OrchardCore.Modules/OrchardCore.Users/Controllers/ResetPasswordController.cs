using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.Modules;
using OrchardCore.RateLimits;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;

namespace OrchardCore.Users.Controllers;

[Feature(UserConstants.Features.ResetPassword)]
public sealed class ResetPasswordController : Controller
{
    private const string ForgotPasswordCooldownCacheKeyPrefix = "ForgotPasswordEmail:";

    private static readonly TimeSpan s_forgotPasswordCooldown = TimeSpan.FromMinutes(1);

    private readonly IUserService _userService;
    private readonly UserManager<IUser> _userManager;
    private readonly ISiteService _siteService;
    private readonly IEnumerable<IPasswordRecoveryFormEvents> _passwordRecoveryFormEvents;
    private readonly IOptionsMonitor<RegistrationOptions> _registrationOptions;
    private readonly ILogger _logger;
    private readonly IUpdateModelAccessor _updateModelAccessor;
    private readonly IDisplayManager<ForgotPasswordForm> _forgotPasswordDisplayManager;
    private readonly UserEmailService _userEmailService;
    private readonly IDisplayManager<ResetPasswordForm> _resetPasswordDisplayManager;
    private readonly IDistributedCache _distributedCache;

    internal readonly IStringLocalizer S;

    public ResetPasswordController(
        IUserService userService,
        UserManager<IUser> userManager,
        ISiteService siteService,
        ILogger<ResetPasswordController> logger,
        IUpdateModelAccessor updateModelAccessor,
        IDisplayManager<ForgotPasswordForm> forgotPasswordDisplayManager,
        UserEmailService userEmailService,
        IDisplayManager<ResetPasswordForm> resetPasswordDisplayManager,
        IEnumerable<IPasswordRecoveryFormEvents> passwordRecoveryFormEvents,
        IOptionsMonitor<RegistrationOptions> registrationOptions,
        IDistributedCache distributedCache,
        IStringLocalizer<ResetPasswordController> stringLocalizer)
    {
        _userService = userService;
        _userManager = userManager;
        _siteService = siteService;
        _logger = logger;
        _updateModelAccessor = updateModelAccessor;
        _forgotPasswordDisplayManager = forgotPasswordDisplayManager;
        _userEmailService = userEmailService;
        _resetPasswordDisplayManager = resetPasswordDisplayManager;
        _passwordRecoveryFormEvents = passwordRecoveryFormEvents;
        _registrationOptions = registrationOptions;
        _distributedCache = distributedCache;
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
    [RateLimitGroup(UserRateLimiterPolicyNames.PasswordRecovery)]
    public async Task<IActionResult> ForgotPasswordPOST()
    {
        var site = await _siteService.GetSiteSettingsAsync();
        if (!site.TryGet<ResetPasswordSettings>(out var settings) || !settings.AllowResetPassword)
        {
            return NotFound();
        }

        var model = new ForgotPasswordForm();

        var formShape = await _forgotPasswordDisplayManager.UpdateEditorAsync(model, _updateModelAccessor.ModelUpdater, false, string.Empty, string.Empty);

        await _passwordRecoveryFormEvents.InvokeAsync((e, modelState) => e.RecoveringPasswordAsync((key, message) => modelState.AddModelError(key, message)), ModelState, _logger);

        if (ModelState.IsValid)
        {
            if (await _userService.GetForgotPasswordUserAsync(model.UsernameOrEmail) is not User user)
            {
                // Redirect to the confirmation page to ensure scrapers cannot determine if a username or email is already registered.
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            if (_registrationOptions.CurrentValue.UsersMustValidateEmail && !await _userManager.IsEmailConfirmedAsync(user))
            {
                ModelState.AddModelError(string.Empty, S["Before you can reset your password, you need to verify your email address."]);
            }
            else if (await TryStartForgotPasswordCooldownAsync(user))
            {
                await _userEmailService.SendPasswordResetAsync(user);

                var context = new PasswordRecoveryContext(user);

                await _passwordRecoveryFormEvents.InvokeAsync((handler, context) => handler.PasswordRecoveredAsync(context), context, _logger);
            }

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
    [ActionName(nameof(ResetPassword))]
    [RateLimitGroup(UserRateLimiterPolicyNames.PasswordRecovery)]
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

    // The per-IP rate limit only applies when the Rate Limits feature is enabled, and it allows the many users who share an IP address
    // to recover their passwords, so this cooldown always limits how often the same user can receive a password reset email.
    // The confirmation page is still shown during the cooldown, so it doesn't reveal whether the account exists.
    private async Task<bool> TryStartForgotPasswordCooldownAsync(IUser user)
    {
        var cooldownKey = ForgotPasswordCooldownCacheKeyPrefix + await _userManager.GetUserIdAsync(user);

        if (await _distributedCache.GetAsync(cooldownKey, HttpContext.RequestAborted) != null)
        {
            return false;
        }

        await _distributedCache.SetAsync(cooldownKey, [1], new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = s_forgotPasswordCooldown,
        }, HttpContext.RequestAborted);

        return true;
    }
}
