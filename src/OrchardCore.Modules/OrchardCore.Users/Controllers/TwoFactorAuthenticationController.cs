using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Authorize]
[Feature(UserConstants.Features.TwoFactorAuthentication)]
public class TwoFactorAuthenticationController : TwoFactorAuthenticationBaseController, IUpdateModel
{
    private readonly ILogger _logger;
    private readonly IEnumerable<ILoginFormEvent> _accountEvents;
    private readonly IdentityOptions _identityOptions;
    private readonly IShapeFactory _shapeFactory;
    private readonly IDisplayManager<TwoFactorMethod> _twoFactorDisplayManager;

    public TwoFactorAuthenticationController(
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ILogger<AccountController> logger,
        ISiteService siteService,
        IHtmlLocalizer<AccountController> htmlLocalizer,
        IStringLocalizer<AccountController> stringLocalizer,
        IOptions<TwoFactorOptions> twoFactorOptions,
        IEnumerable<ILoginFormEvent> accountEvents,
        INotifier notifier,
        IDistributedCache distributedCache,
        IOptions<IdentityOptions> identityOptions,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorHandlerCoordinator,
        IShapeFactory shapeFactory,
        IDisplayManager<TwoFactorMethod> twoFactorDisplayManager)
        : base(userManager,
            distributedCache,
            signInManager,
            twoFactorHandlerCoordinator,
            notifier,
            siteService,
            htmlLocalizer,
            stringLocalizer,
            twoFactorOptions)
    {
        _logger = logger;
        _accountEvents = accountEvents;
        _identityOptions = identityOptions.Value;
        _shapeFactory = shapeFactory;
        _twoFactorDisplayManager = twoFactorDisplayManager;
    }

    [AllowAnonymous]
    public async Task<IActionResult> LoginWithTwoFactorAuthentication(bool rememberMe = false, bool next = false, string provider = "", string returnUrl = "")
    {
        // Ensure the user has gone through the username & password screen first.
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            return RedirectToAccountLogin();
        }

        var providers = await GetTwoFactorProvidersAsync(user);

        var currentProvider = GetProvider(providers, user, provider, next);

        if (String.IsNullOrEmpty(currentProvider))
        {
            await Notifier.ErrorAsync(H["Unable to find an active two-factor provider."]);

            return RedirectToAccountLogin();
        }

        var twoFactorSettings = (await SiteService.GetSiteSettingsAsync()).As<TwoFactorLoginSettings>();

        var model = new LoginWithTwoFactorAuthenticationViewModel
        {
            HasMultipleProviders = providers.Count > 1,
            CurrentProvider = currentProvider,
            RememberMe = rememberMe,
            ReturnUrl = returnUrl,
            AllowRememberDevice = twoFactorSettings.AllowRememberClientTwoFactorAuthentication,
        };

        return View(model);
    }

    [HttpPost, AllowAnonymous, ActionName(nameof(LoginWithTwoFactorAuthentication))]
    public async Task<IActionResult> LoginWithTwoFactorAuthenticationPost(LoginWithTwoFactorAuthenticationViewModel model)
    {
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();

        if (user == null)
        {
            return RedirectToAccountLogin();
        }

        var providers = await GetTwoFactorProvidersAsync(user);

        var currentProvider = GetProvider(providers, user, model.CurrentProvider, false);

        if (String.IsNullOrEmpty(currentProvider))
        {
            await Notifier.ErrorAsync(H["Unable to find an active two-factor provider."]);

            return RedirectToAccountLogin();
        }

        if (ModelState.IsValid)
        {
            var twoFactorSettings = (await SiteService.GetSiteSettingsAsync()).As<TwoFactorLoginSettings>();
            var rememberDevice = twoFactorSettings.AllowRememberClientTwoFactorAuthentication && model.RememberDevice;

            var authenticatorCode = StripToken(model.VerificationCode);
            var result = await SignInManager.TwoFactorSignInAsync(currentProvider, authenticatorCode, model.RememberMe, rememberDevice);

            if (result.Succeeded)
            {
                await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

                return await LoggedInActionResultAsync(user, model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                ModelState.AddModelError(String.Empty, S["The account is locked out."]);
                await _accountEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

                return RedirectToAccountLogin();
            }

            ModelState.AddModelError(String.Empty, S["Invalid verification code."]);

            // Login failed with a known user.
            await _accountEvents.InvokeAsync((e, user) => e.LoggingInFailedAsync(user), user, _logger);
        }

        model.HasMultipleProviders = providers.Count > 1;

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> LoginWithRecoveryCode(string returnUrl = null)
    {
        // Ensure the user has gone through the username & password screen first
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();
        if (user == null)
        {
            return RedirectToAccountLogin();
        }

        return View(new LoginWithRecoveryCodeViewModel()
        {
            ReturnUrl = returnUrl,
        });
    }

    [HttpPost, AllowAnonymous]
    public async Task<IActionResult> LoginWithRecoveryCode(LoginWithRecoveryCodeViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Ensure the user has gone through the username & password screen first.
            var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();

            if (user == null)
            {
                return RedirectToAccountLogin();
            }

            var recoveryCode = model.RecoveryCode.Replace(" ", String.Empty);

            var result = await SignInManager.TwoFactorRecoveryCodeSignInAsync(recoveryCode);

            if (result.Succeeded)
            {
                await _accountEvents.InvokeAsync((e, user) => e.LoggedInAsync(user), user, _logger);

                return await LoggedInActionResultAsync(user, model.ReturnUrl);
            }

            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");

                ModelState.AddModelError(String.Empty, S["The account is locked out"]);
                await _accountEvents.InvokeAsync((e, user) => e.IsLockedOutAsync(user), user, _logger);

                return RedirectToAccountLogin();
            }

            _logger.LogWarning("Invalid recovery code entered for user.");
            ModelState.AddModelError(String.Empty, S["Invalid recovery code entered."]);
        }

        return View(model);
    }

    [Admin]
    public async Task<IActionResult> Index()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var providers = await GetTwoFactorProvidersAsync(user);
        var model = new TwoFactorAuthenticationViewModel();
        await PopulateModelAsync(user, providers, model);

        if (user is User u)
        {
            model.PreferredProvider = u.As<TwoFactorPreference>().DefaultProvider;
        }

        return View(model);
    }

    [HttpPost, Admin]
    public async Task<IActionResult> Index(TwoFactorAuthenticationViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var providers = await GetTwoFactorProvidersAsync(user);

        if (ModelState.IsValid)
        {
            if (user is User u && providers.Contains(model.PreferredProvider))
            {
                u.Put(new TwoFactorPreference()
                {
                    DefaultProvider = model.PreferredProvider
                });

                await UserManager.UpdateAsync(u);

                await Notifier.SuccessAsync(H["Preferences were updated successfully."]);

                return RedirectToAction(nameof(Index));
            }

            await Notifier.ErrorAsync(H["Unable to update preferences."]);
        }

        await PopulateModelAsync(user, providers, model);

        return View(model);
    }

    [HttpPost, Admin]
    public async Task<IActionResult> ForgetTwoFactorClient()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        await SignInManager.ForgetTwoFactorClientAsync();
        await Notifier.SuccessAsync(H["The current browser has been forgotten. When you login again from this browser you will be prompted for your 2fa code."]);

        return RedirectToAction(nameof(Index));
    }

    [Admin]
    public async Task<IActionResult> GenerateRecoveryCodes()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var isTwoFactorEnabled = await UserManager.GetTwoFactorEnabledAsync(user);
        if (!isTwoFactorEnabled)
        {
            await Notifier.ErrorAsync(H["Cannot generate recovery codes for user because they do not have 2FA enabled."]);

            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost, Admin, ActionName(nameof(GenerateRecoveryCodes))]
    public async Task<IActionResult> GenerateRecoveryCodesPost()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        if (!await UserManager.GetTwoFactorEnabledAsync(user))
        {
            await Notifier.ErrorAsync(H["Cannot generate recovery codes for user because they do not have 2FA enabled."]);

            return RedirectToAction(nameof(Index));
        }

        var twoFactorSettings = (await SiteService.GetSiteSettingsAsync()).As<TwoFactorLoginSettings>();
        var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, twoFactorSettings.NumberOfRecoveryCodesToGenerate);
        await SetRecoveryCodesAsync(recoveryCodes.ToArray(), await UserManager.GetUserIdAsync(user));

        await Notifier.SuccessAsync(H["You have generated new recovery codes."]);

        return RedirectToAction(nameof(ShowRecoveryCodes));
    }

    [Admin]
    public async Task<IActionResult> ShowRecoveryCodes()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var userId = await UserManager.GetUserIdAsync(user);

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

    [Admin, HttpPost]
    public async Task<IActionResult> EnableTwoFactorAuthentication()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var providers = await GetTwoFactorProvidersAsync(user);

        if (providers.Count == 0)
        {
            await Notifier.ErrorAsync(H["To enable two-factor authentication, enable at least one two-factor method."]);

            return RedirectToTwoFactorIndex();
        }

        await EnableTwoFactorAuthenticationAsync(user);

        await Notifier.SuccessAsync(H["Two-factor authentication has been enabled."]);

        return await RedirectToTwoFactorAsync(user);
    }

    [Admin]
    public async Task<IActionResult> DisableTwoFactorAuthentication()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        if (await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync())
        {
            await Notifier.WarningAsync(H["Two-factor authentication cannot be disabled for the current user."]);

            return RedirectToAction(nameof(Index));
        }

        return View();
    }

    [HttpPost, Admin, ActionName(nameof(DisableTwoFactorAuthentication))]
    public async Task<IActionResult> DisableTwoFactorAuthenticationPost()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        if (await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync())
        {
            await Notifier.WarningAsync(H["Two-factor authentication cannot be disabled for the current user."]);

            return RedirectToAction(nameof(Index));
        }

        var disableResult = await UserManager.SetTwoFactorEnabledAsync(user, false);
        if (!disableResult.Succeeded)
        {
            await Notifier.ErrorAsync(H["Unexpected error occurred disabling two-factor authentication."]);
        }
        else
        {
            await Notifier.WarningAsync(H["Two-factor authentication has been disabled."]);
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<string[]> GetCachedRecoveryCodes(string userId)
    {
        var key = GetRecoveryCodesCacheKey(userId);

        var data = await DistributedCache.GetAsync(key);

        if (data != null && data.Length > 0)
        {
            var model = JsonSerializer.Deserialize<ShowRecoveryCodesViewModel>(data);

            return model?.RecoveryCodes ?? Array.Empty<string>();
        }

        return Array.Empty<string>();
    }

    private static string GetProvider(IList<string> providers, IUser user, string provider = null, bool next = false)
    {
        var validProviderRequested = !String.IsNullOrEmpty(provider) && providers.Contains(provider);
        var defaultProvider = validProviderRequested ? provider : providers.FirstOrDefault();

        if (!validProviderRequested && user is User u)
        {
            // At this point, no or invalid provider was given. Check the user preference and load the default provider if available.
            var preferences = u.As<TwoFactorPreference>();

            if (!String.IsNullOrEmpty(preferences.DefaultProvider) && providers.Contains(preferences.DefaultProvider))
            {
                defaultProvider = preferences.DefaultProvider;
            }
        }

        if (next && providers.Count > 1)
        {
            // At this point, the user has multiple enabled providers and we are looking for the next provider.
            var index = providers.IndexOf(defaultProvider);

            if (index + 1 < providers.Count)
            {
                defaultProvider = providers[index + 1];
            }
            else
            {
                defaultProvider = providers[0];
            }
        }

        return defaultProvider;
    }

    private async Task PopulateModelAsync(IUser user, IList<string> providers, TwoFactorAuthenticationViewModel model)
    {
        model.User = user;
        model.IsTwoFaEnabled = await UserManager.GetTwoFactorEnabledAsync(user);
        model.IsMachineRemembered = await SignInManager.IsTwoFactorClientRememberedAsync(user);
        model.RecoveryCodesLeft = await UserManager.CountRecoveryCodesAsync(user);
        model.CanDisableTwoFactor = !await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync();
        model.ValidTwoFactorProviders = providers.Select(providerName => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(providerName, providerName)).ToList();

        foreach (var key in TwoFactorOptions.Providers)
        {
            var method = new TwoFactorMethod()
            {
                Provider = key,
                IsEnabled = providers.Contains(key),
            };

            var shape = await _twoFactorDisplayManager.BuildDisplayAsync(method, this, "SummaryAdmin");

            model.AuthenticationMethods.Add(shape);
        }
    }
}
