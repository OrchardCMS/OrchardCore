using System.Buffers.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.RateLimits;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Authorize]
[Feature(UserConstants.Features.Passkeys)]
public sealed class PasskeyController : TwoFactorAuthenticationBaseController
{
    private const string JsonContentType = "application/json";

    private static readonly string s_accountControllerName = typeof(AccountController).ControllerName();
    private static readonly string s_twoFactorControllerName = typeof(TwoFactorAuthenticationController).ControllerName();

    private readonly PasskeyService _passkeyService;
    private readonly IEnumerable<ILoginFormEvent> _loginFormEvents;
    private readonly IdentityPasskeyOptions _passkeyOptions;
    private readonly ILogger _logger;

    public PasskeyController(
        PasskeyService passkeyService,
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        IEnumerable<ILoginFormEvent> loginFormEvents,
        IOptions<IdentityPasskeyOptions> passkeyOptions,
        IOptions<TwoFactorOptions> twoFactorOptions,
        ISiteService siteService,
        INotifier notifier,
        IDistributedCache distributedCache,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorAuthenticationHandlerCoordinator,
        ILogger<PasskeyController> logger,
        IHtmlLocalizer<PasskeyController> htmlLocalizer,
        IStringLocalizer<PasskeyController> stringLocalizer)
        : base(
            userManager,
            distributedCache,
            signInManager,
            twoFactorAuthenticationHandlerCoordinator,
            notifier,
            siteService,
            htmlLocalizer,
            stringLocalizer,
            twoFactorOptions)
    {
        _passkeyService = passkeyService;
        _loginFormEvents = loginFormEvents;
        _passkeyOptions = passkeyOptions.Value;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        var user = await UserManager.GetUserAsync(User);

        if (user is not User u)
        {
            return UserNotFound();
        }

        var model = new PasskeysViewModel
        {
            IsTwoFactorMethod = IsTwoFactorMethod,
            IsTwoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(user),
            Passkeys = u.Passkeys
                .OrderBy(passkey => passkey.CreatedUtc)
                .Select(passkey => new PasskeyEntryViewModel
                {
                    Id = Base64Url.EncodeToString(passkey.CredentialId),
                    Name = passkey.Name,
                    CreatedUtc = passkey.CreatedUtc,
                    LastUsedUtc = passkey.LastUsedUtc,
                    IsBackedUp = passkey.IsBackedUp,
                })
                .ToList(),
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreationOptions()
    {
        var user = await UserManager.GetUserAsync(User);

        if (user == null)
        {
            return UserNotFound();
        }

        return Content(await _passkeyService.MakeCreationOptionsAsync(user), JsonContentType);
    }

    [HttpPost]
    public async Task<IActionResult> Create(AddPasskeyViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);

        if (user == null)
        {
            return UserNotFound();
        }

        if (!ModelState.IsValid)
        {
            await Notifier.ErrorAsync(H["The passkey couldn't be added."]);

            return RedirectToAction(nameof(Index));
        }

        var result = await _passkeyService.PerformAttestationAsync(model.Credential);

        if (!result.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Warning))
            {
                _logger.LogWarning("A passkey couldn't be added: {Reason}", result.Failure.Message);
            }

            await Notifier.ErrorAsync(H["The passkey couldn't be added. It may already be registered."]);

            return RedirectToAction(nameof(Index));
        }

        // The options were created for the current user, make sure the credential was created for them too.
        if (!string.Equals(result.UserEntity.Id, await UserManager.GetUserIdAsync(user), StringComparison.Ordinal))
        {
            await Notifier.ErrorAsync(H["The passkey couldn't be added."]);

            return RedirectToAction(nameof(Index));
        }

        var passkeys = await UserManager.GetPasskeysAsync(user);

        result.Passkey.Name = string.IsNullOrWhiteSpace(model.Name)
            ? S["Passkey {0}", passkeys.Count + 1]
            : model.Name.Trim();

        var identityResult = await UserManager.AddOrUpdatePasskeyAsync(user, result.Passkey);

        if (!identityResult.Succeeded)
        {
            await Notifier.ErrorAsync(H["The passkey couldn't be added."]);

            return RedirectToAction(nameof(Index));
        }

        await Notifier.SuccessAsync(H["The passkey was added."]);

        // A passkey is enough to meet a two-factor authentication requirement.
        if (IsTwoFactorMethod &&
            !await UserManager.GetTwoFactorEnabledAsync(user) &&
            await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync(user))
        {
            await EnableTwoFactorAuthenticationAsync(user);
            await Notifier.SuccessAsync(H["Two-factor authentication has been enabled."]);

            return await RedirectToTwoFactorAsync(user);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Rename(RenamePasskeyViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);

        if (user == null)
        {
            return UserNotFound();
        }

        var passkey = await FindPasskeyAsync(user, model.Id);

        if (passkey == null || !ModelState.IsValid)
        {
            await Notifier.ErrorAsync(H["The passkey couldn't be renamed."]);

            return RedirectToAction(nameof(Index));
        }

        passkey.Name = model.Name.Trim();

        await UserManager.AddOrUpdatePasskeyAsync(user, passkey);
        await Notifier.SuccessAsync(H["The passkey was renamed."]);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var user = await UserManager.GetUserAsync(User);

        if (user == null)
        {
            return UserNotFound();
        }

        var passkey = await FindPasskeyAsync(user, id);

        if (passkey == null)
        {
            await Notifier.ErrorAsync(H["The passkey couldn't be removed."]);

            return RedirectToAction(nameof(Index));
        }

        async Task RemovePasskeyAsync()
        {
            await UserManager.RemovePasskeyAsync(user, passkey.CredentialId);
            await Notifier.SuccessAsync(H["The passkey was removed."]);
        }

        // Removing the last passkey removes the passkey two-factor method.
        if (IsTwoFactorMethod &&
            await UserManager.GetTwoFactorEnabledAsync(user) &&
            (await UserManager.GetPasskeysAsync(user)).Count == 1 &&
            (await GetTwoFactorProvidersAsync(user)).Contains(UserConstants.PasskeyTwoFactorProvider))
        {
            return await RemoveTwoFactorProviderAsync(user, RemovePasskeyAsync);
        }

        await RemovePasskeyAsync();

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> RequestOptions()
    {
        var loginSettings = await SiteService.GetSettingsAsync<LoginSettings>();

        if (loginSettings.DisableLocalLogin)
        {
            return BadRequest();
        }

        // Any passkey of the site can be used, so that the user doesn't have to type their username.
        return Content(await _passkeyService.MakeRequestOptionsAsync(null), JsonContentType);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> TwoFactorRequestOptions()
    {
        // Only the passkeys of the user who entered their password can be used as the second factor.
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            return BadRequest();
        }

        return Content(await _passkeyService.MakeRequestOptionsAsync(user), JsonContentType);
    }

    [HttpPost]
    [AllowAnonymous]
    [RateLimitGroup(UserRateLimiterPolicyNames.PasswordAuthentication)]
    public async Task<IActionResult> Login(PasskeyLoginViewModel model, string returnUrl = null)
    {
        var loginSettings = await SiteService.GetSettingsAsync<LoginSettings>();

        if (loginSettings.DisableLocalLogin)
        {
            return LoginError(S["Local login is disabled."], returnUrl);
        }

        var result = await _passkeyService.PerformAssertionAsync(model.Credential);

        if (!result.Succeeded)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("A passkey sign-in failed: {Reason}", result.Failure.Message);
            }

            return LoginError(S["The passkey couldn't be verified."], returnUrl);
        }

        var user = result.User;

        if (await UserManager.IsLockedOutAsync(user))
        {
            await _loginFormEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

            return LoginError(S["The account is locked out"], returnUrl);
        }

        if (!await SignInManager.CanSignInAsync(user))
        {
            await _loginFormEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);

            return LoginError(S["Invalid login attempt."], returnUrl);
        }

        foreach (var loginFormEvent in _loginFormEvents)
        {
            var loginResult = await loginFormEvent.ValidatingLoginAsync(user);

            if (loginResult != null)
            {
                return loginResult;
            }
        }

        if (!(await _passkeyService.UpdateUsedPasskeyAsync(user, result.Passkey)).Succeeded)
        {
            return LoginError(S["Invalid login attempt."], returnUrl);
        }

        var rememberMe = loginSettings.AllowRememberMe
            ? model.RememberMe
            : loginSettings.UsePersistentAuthenticationCookie;

        // With user verification, the passkey proves both possession of the device and the biometric or PIN check.
        var isMultiFactor = string.Equals(_passkeyOptions.UserVerificationRequirement, "required", StringComparison.OrdinalIgnoreCase);

        if (!isMultiFactor &&
            await UserManager.GetTwoFactorEnabledAsync(user) &&
            !await SignInManager.IsTwoFactorClientRememberedAsync(user))
        {
            // Without user verification, a passkey is a single factor, so ask for the second one as after a password.
            var twoFactorIdentity = new ClaimsIdentity(IdentityConstants.TwoFactorUserIdScheme);
            twoFactorIdentity.AddClaim(new Claim(ClaimTypes.Name, await UserManager.GetUserIdAsync(user)));

            await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme, new ClaimsPrincipal(twoFactorIdentity));

            return RedirectToAction(
                nameof(TwoFactorAuthenticationController.LoginWithTwoFactorAuthentication),
                s_twoFactorControllerName,
                new
                {
                    returnUrl,
                    rememberMe,
                });
        }

        List<Claim> claims = [new Claim(UserConstants.AuthenticationMethodsClaimType, UserConstants.ProofOfPossessionAuthenticationMethod)];

        if (isMultiFactor)
        {
            claims.Add(new Claim(UserConstants.AuthenticationMethodsClaimType, UserConstants.MultiFactorAuthenticationMethod));
        }

        await SignInManager.SignInWithClaimsAsync(user, rememberMe, claims);

        _logger.LogInformation("User logged in with a passkey.");

        await _loginFormEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

        return await LoggedInActionResultAsync(user, returnUrl);
    }

    private bool IsTwoFactorMethod
        => TwoFactorOptions.Providers.Contains(UserConstants.PasskeyTwoFactorProvider);

    private async Task<UserPasskeyInfo> FindPasskeyAsync(IUser user, string id)
    {
        if (string.IsNullOrEmpty(id) || !Base64Url.IsValid(id))
        {
            return null;
        }

        return await UserManager.GetPasskeyAsync(user, Base64Url.DecodeFromChars(id));
    }

    private RedirectToActionResult LoginError(string message, string returnUrl)
    {
        TempData["error"] = message;

        return RedirectToAction(nameof(AccountController.Login), s_accountControllerName, new { returnUrl });
    }
}
