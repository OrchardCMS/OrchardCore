using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Authorize]
public class TwoFactorAuthenticationController : AccountBaseController
{
    private const string _authenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&issuer={0}&digits={3}";

    private readonly SignInManager<IUser> _signInManager;
    private readonly ILogger _logger;
    private readonly ISiteService _siteService;
    private readonly IEnumerable<ILoginFormEvent> _accountEvents;
    private readonly INotifier _notifier;
    private readonly IDistributedCache _distributedCache;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly UrlEncoder _urlEncoder;
    private readonly ShellSettings _shellSettings;
    private readonly IHtmlLocalizer H;
    private readonly IStringLocalizer S;

    public TwoFactorAuthenticationController(
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ILogger<AccountController> logger,
        ISiteService siteService,
        IHtmlLocalizer<AccountController> htmlLocalizer,
        IStringLocalizer<AccountController> stringLocalizer,
        IEnumerable<ILoginFormEvent> accountEvents,
        INotifier notifier,
        IDistributedCache distributedCache,
        IDataProtectionProvider dataProtectionProvider,
        UrlEncoder urlEncoder,
        ShellSettings shellSettings)
        : base(userManager)
    {
        _signInManager = signInManager;
        _logger = logger;
        _siteService = siteService;
        _accountEvents = accountEvents;
        _notifier = notifier;
        _distributedCache = distributedCache;
        _dataProtectionProvider = dataProtectionProvider;
        _urlEncoder = urlEncoder;
        _shellSettings = shellSettings;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> LoginWith2FA(bool rememberMe, string returnUrl = null)
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        // Ensure the user has gone through the username & password screen first.
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            return RedirectToAction(nameof(AccountController.Login), typeof(AccountController).ControllerName());
        }

        var model = new LoginWith2FAViewModel()
        {
            RememberMe = rememberMe,
            ReturnUrl = returnUrl,
        };

        return View(model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWith2FA(LoginWith2FAViewModel model)
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            return RedirectToAction(nameof(AccountController.Login), typeof(AccountController).ControllerName());
        }

        var authenticatorCode = StripToken(model.TwoFactorCode);

        var rememberClient = loginSettings.AllowRememberClientTwoFactorAuthentication && model.RememberClient;

        var result = await _signInManager.TwoFactorAuthenticatorSignInAsync(authenticatorCode, model.RememberMe, rememberClient);

        var userId = await _userManager.GetUserIdAsync(user);

        if (result.Succeeded)
        {
            _logger.LogInformation(1, "User with ID '{UserId}' logged in with 2FA.", userId);

            await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

            return await LoggedInActionResult(user, model.ReturnUrl);
        }

        if (result.IsLockedOut)
        {
            _logger.LogWarning("User account locked out.");

            ModelState.AddModelError(String.Empty, S["The account is locked out"]);
            await _accountEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

            return RedirectToAction(nameof(AccountController.Login), typeof(AccountController).ControllerName());
        }

        _logger.LogWarning("Invalid authenticator code entered for user with ID '{UserId}'.", userId);

        ModelState.AddModelError(String.Empty, S["Invalid authenticator code."]);

        // Login failed with a known user.
        await _accountEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        // Ensure the user has gone through the username & password screen first
        var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToAction(nameof(AccountController.Login), typeof(AccountController).ControllerName());
        }

        return View(new LoginWithRecoveryCodeViewModel()
        {
            ReturnUrl = returnUrl,
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model)
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            // Ensure the user has gone through the username & password screen first.
            var user = await _signInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return RedirectToAction(nameof(AccountController.Login), typeof(AccountController).ControllerName());
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", String.Empty);

            var result = await _signInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            var userId = await _userManager.GetUserIdAsync(user);

            if (result.Succeeded)
            {
                _logger.LogInformation("User with ID '{UserId}' logged in with a recovery code.", userId);

                await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

                return await LoggedInActionResult(user, model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");

                ModelState.AddModelError(String.Empty, S["The account is locked out"]);
                await _accountEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

                return RedirectToAction(nameof(AccountController.Login), typeof(AccountController).ControllerName());
            }

            _logger.LogWarning("Invalid recovery code entered for user with ID '{UserId}' ", userId);

            ModelState.AddModelError(String.Empty, S["Invalid recovery code entered."]);
        }

        return View(model);
    }

    [Admin]
    public async Task<IActionResult> EnableAuthenticator(string returnUrl)
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var model = await LoadSharedKeyAndQrCodeUriAsync(user, loginSettings);

        model.ReturnUrl = returnUrl;

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Admin]
    public async Task<IActionResult> EnableAuthenticator(EnableAuthenticatorViewModel model)
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.EnableTwoFactorAuthentication && !loginSettings.EnableTwoFactorAuthenticationForSpecificRoles)
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (!ModelState.IsValid)
        {
            return View(await LoadSharedKeyAndQrCodeUriAsync(user, loginSettings));
        }

        var verificationCode = StripToken(model.Code);

        var provider = _userManager.Options.Tokens.AuthenticatorTokenProvider;

        var is2faTokenValid = await _userManager.VerifyTwoFactorTokenAsync(user, provider, verificationCode);

        if (!is2faTokenValid)
        {
            ModelState.AddModelError(nameof(model.Code), S["Verification code is invalid."]);

            return View(await LoadSharedKeyAndQrCodeUriAsync(user, loginSettings));
        }

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var userId = await _userManager.GetUserIdAsync(user);

        _logger.LogInformation("User with ID '{UserId}' has enabled 2FA with an authenticator app.", userId);

        var twoFactorClaim = User.Claims
            .FirstOrDefault(claim => claim.Type == TwoFactorAuthenticationClaimsProvider.TwoFactorAuthenticationClaimType);

        if (twoFactorClaim != null)
        {
            await _userManager.RemoveClaimAsync(user, twoFactorClaim);
            await _signInManager.RefreshSignInAsync(user);
        }

        await _notifier.SuccessAsync(H["Your authenticator app has been verified."]);

        if (await _userManager.CountRecoveryCodesAsync(user) == 0)
        {
            var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, loginSettings.NumberOfRecoveryCodesToGenerate);
            await SetRecoveryCodes(recoveryCodes.ToArray(), userId);

            return RedirectToAction(nameof(ShowRecoveryCodes));
        }

        return RedirectToAction(nameof(Index));
    }

    [Admin]
    public async Task<IActionResult> Index()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var model = new TwoFactorAuthenticationViewModel()
        {
            HasAuthenticator = await _userManager.GetAuthenticatorKeyAsync(user) != null,
            Is2faEnabled = await _userManager.GetTwoFactorEnabledAsync(user),
            IsMachineRemembered = await _signInManager.IsTwoFactorClientRememberedAsync(user),
            RecoveryCodesLeft = await _userManager.CountRecoveryCodesAsync(user),
            CanDisable2Fa = !loginSettings.RequireTwoFactorAuthentication,
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Admin]
    public async Task<IActionResult> ForgetTwoFactorClient()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await _signInManager.ForgetTwoFactorClientAsync();

        await _notifier.SuccessAsync(H["The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code."]);

        return RedirectToAction(nameof(Index));
    }

    [Admin]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
        {
            await _notifier.ErrorAsync(H["Cannot generate recovery codes for user because they do not have 2FA enabled."]);

            return RedirectToAction(nameof(EnableAuthenticator));
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Admin]
    [ActionName(nameof(GenerateRecoveryCodes))]
    public async Task<IActionResult> GenerateRecoveryCodesPost()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var isTwoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
        {
            await _notifier.ErrorAsync(H["Cannot generate recovery codes for user because they do not have 2FA enabled."]);

            return RedirectToAction(nameof(EnableAuthenticator));
        }

        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, loginSettings.NumberOfRecoveryCodesToGenerate);
        var userId = await _userManager.GetUserIdAsync(user);
        await SetRecoveryCodes(recoveryCodes.ToArray(), userId);

        _logger.LogInformation("User with ID '{UserId}' has generated new 2FA recovery codes.", userId);

        await _notifier.SuccessAsync(H["You have generated new recovery codes."]);

        return RedirectToAction(nameof(ShowRecoveryCodes));
    }

    [Admin]
    public async Task<IActionResult> ShowRecoveryCodes()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        var userId = await _userManager.GetUserIdAsync(user);

        var recoveryCodes = await GetCachedRecoveryCodes(userId);

        if (recoveryCodes == null || recoveryCodes.Length == 0)
        {
            return RedirectToAction(nameof(Index));
        }

        return View(new ShowRecoveryCodesViewModel()
        {
            RecoveryCodes = recoveryCodes,
        });
    }

    [Admin]
    public async Task<IActionResult> ResetAuthenticator()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Admin]
    [ActionName(nameof(ResetAuthenticator))]
    public async Task<IActionResult> ResetAuthenticatorPost()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);

        _logger.LogInformation("User with ID '{UserId}' has reset their authentication app key.", await _userManager.GetUserIdAsync(user));

        await _signInManager.RefreshSignInAsync(user);
        await _notifier.SuccessAsync(H["Your authenticator app key has been reset, you will need to configure your authenticator app using the new key."]);

        return RedirectToAction(nameof(EnableAuthenticator));
    }

    [Admin]
    public async Task<IActionResult> Disable2FA()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (loginSettings.RequireTwoFactorAuthentication)
        {
            await _notifier.WarningAsync(H["Two-factor authentication cannot be disabled for the current user."]);

            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Admin]
    [ActionName(nameof(Disable2FA))]
    public async Task<IActionResult> Disable2FAPost()
    {
        var loginSettings = (await _siteService.GetSiteSettingsAsync()).As<LoginSettings>();

        if (!loginSettings.IsTwoFactorAuthenticationEnabled())
        {
            return NotFound();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
        }

        if (loginSettings.RequireTwoFactorAuthentication)
        {
            await _notifier.WarningAsync(H["Two-factor authentication cannot be disabled for the current user."]);

            return RedirectToAction(nameof(Index));
        }

        var disable2faResult = await _userManager.SetTwoFactorEnabledAsync(user, false);
        if (!disable2faResult.Succeeded)
        {
            await _notifier.ErrorAsync(H["Unexpected error occurred disabling two-factor authentication."]);
        }
        else
        {
            _logger.LogInformation("User with ID '{UserId}' has disabled 2fa.", _userManager.GetUserId(User));
            await _notifier.WarningAsync(H["Two-factor authentication has been disabled. You can re-enable it when you setup an authenticator app"]);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task SetRecoveryCodes(string[] codes, string userId)
    {
        var key = GetRecoveryCodesCacheKey(userId);

        var model = new ShowRecoveryCodesViewModel()
        {
            RecoveryCodes = codes ?? Array.Empty<string>(),
        };

        var data = JsonSerializer.SerializeToUtf8Bytes(model);

        await _distributedCache.SetAsync(key, data,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 5)
            });
    }

    private async Task<string[]> GetCachedRecoveryCodes(string userId)
    {
        var key = GetRecoveryCodesCacheKey(userId);

        var data = await _distributedCache.GetAsync(key);

        if (data != null && data.Length > 0)
        {
            var model = JsonSerializer.Deserialize<ShowRecoveryCodesViewModel>(data);

            return model?.RecoveryCodes ?? Array.Empty<string>();
        }

        return Array.Empty<string>();
    }

    private static string GetRecoveryCodesCacheKey(string userId)
    {
        return $"TwoFactorAuthenticationRecoveryCodes_{userId}";
    }

    private async Task<EnableAuthenticatorViewModel> LoadSharedKeyAndQrCodeUriAsync(IUser user, LoginSettings settings)
    {
        // Load the authenticator key & QR code URI to display on the form.
        var unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);

        if (String.IsNullOrEmpty(unformattedKey))
        {
            await _userManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await _userManager.GetAuthenticatorKeyAsync(user);
        }

        var displayName = await GetUserDisplayName(user, settings.UseEmailAsAuthenticatorDisplayName);

        return new EnableAuthenticatorViewModel()
        {
            SharedKey = FormatKey(unformattedKey),
            AuthenticatorUri = await GenerateQrCodeUriAsync(displayName, unformattedKey, settings.TokenLength),
        };
    }

    private async Task<string> GetUserDisplayName(IUser user, bool showEmail)
    {
        if (showEmail)
        {
            return await _userManager.GetUserNameAsync(user);
        }

        return await _userManager.GetEmailAsync(user);
    }

    private static string FormatKey(string unformattedKey)
    {
        var result = new StringBuilder();
        var currentPosition = 0;
        while (currentPosition + 4 < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition, 4)).Append(' ');
            currentPosition += 4;
        }
        if (currentPosition < unformattedKey.Length)
        {
            result.Append(unformattedKey.AsSpan(currentPosition));
        }

        return result.ToString().ToLowerInvariant();
    }

    private static string StripToken(string code)
    {
        // Strip spaces and hyphens.
        return code.Replace(" ", String.Empty).Replace("-", String.Empty);
    }

    private async Task<string> GenerateQrCodeUriAsync(string username, string unformattedKey, int tokenLength)
    {
        var site = await _siteService.GetSiteSettingsAsync();

        var name = site.SiteName;

        if (String.IsNullOrWhiteSpace(name))
        {
            name = _shellSettings.Name;
        }

        return String.Format(
            CultureInfo.InvariantCulture,
            _authenticatorUriFormat,
            _urlEncoder.Encode(name),
            _urlEncoder.Encode(username),
            unformattedKey,
            tokenLength);
    }
}
