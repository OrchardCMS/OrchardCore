using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

public abstract class TwoFactorAuthenticationBaseController : AccountBaseController
{
    private static readonly string _twoFactorAuthenticationControllerName = typeof(TwoFactorAuthenticationController).ControllerName();
    private static readonly string _accountControllerName = typeof(AccountController).ControllerName();

    protected readonly UserManager<IUser> UserManager;
    protected readonly IDistributedCache DistributedCache;
    protected readonly SignInManager<IUser> SignInManager;
    protected readonly ITwoFactorAuthenticationHandlerCoordinator TwoFactorAuthenticationHandlerCoordinator;
    protected readonly INotifier Notifier;
    protected readonly ISiteService SiteService;
    protected readonly IHtmlLocalizer H;
    protected readonly IStringLocalizer S;
    protected readonly TwoFactorOptions TwoFactorOptions;

    public TwoFactorAuthenticationBaseController(
        UserManager<IUser> userManager,
        IDistributedCache distributedCache,
        SignInManager<IUser> signInManager,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorAuthenticationHandlerCoordinator,
        INotifier notifier,
        ISiteService siteService,
        IHtmlLocalizer htmlLocalizer,
        IStringLocalizer stringLocalizer,
        IOptions<TwoFactorOptions> twoFactorOptions)
    {
        UserManager = userManager;
        DistributedCache = distributedCache;
        SignInManager = signInManager;
        TwoFactorAuthenticationHandlerCoordinator = twoFactorAuthenticationHandlerCoordinator;
        Notifier = notifier;
        SiteService = siteService;
        TwoFactorOptions = twoFactorOptions.Value;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    protected async Task SetRecoveryCodes(string[] codes, string userId)
    {
        var key = GetRecoveryCodesCacheKey(userId);

        var model = new ShowRecoveryCodesViewModel()
        {
            RecoveryCodes = codes ?? Array.Empty<string>(),
        };

        var data = JsonSerializer.SerializeToUtf8Bytes(model);

        await DistributedCache.SetAsync(key, data,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 5)
            });
    }

    protected async Task<IActionResult> RemoveTwoFactorProviderAync(IUser user, Func<Task> onSuccessAsync)
    {
        var currentProviders = await AvailableProvidersAsync(user);

        if (currentProviders.Count == 1)
        {
            if (await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync())
            {
                await Notifier.ErrorAsync(H["You cannot remove the only active method."]);

                return RedirectToTwoFactorIndex();
            }

            await UserManager.SetTwoFactorEnabledAsync(user, false);

            await Notifier.WarningAsync(H["Your two-factor authentication has been disabled."]);
        }

        await onSuccessAsync();
        await SignInManager.RefreshSignInAsync(user);

        return RedirectToTwoFactorIndex();
    }

    protected async Task EnableTwoFactorAuthentication(IUser user)
    {
        if (await UserManager.GetTwoFactorEnabledAsync(user))
        {
            return;
        }

        await UserManager.SetTwoFactorEnabledAsync(user, true);

        if (await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync())
        {
            await RefreshTwoFactorClaim(user);
        }
    }

    protected async Task RefreshTwoFactorClaim(IUser user)
    {
        var twoFactorClaim = (await UserManager.GetClaimsAsync(user))
            .FirstOrDefault(claim => claim.Type == UserConstants.TwoFactorAuthenticationClaimType);

        if (twoFactorClaim != null)
        {
            await UserManager.RemoveClaimAsync(user, twoFactorClaim);
            await SignInManager.RefreshSignInAsync(user);
        }
    }

    protected async Task<IActionResult> RedirectToTwoFactorAsync(IUser user)
    {
        if (await UserManager.CountRecoveryCodesAsync(user) == 0)
        {
            var twoFactorSettings = (await SiteService.GetSiteSettingsAsync()).As<TwoFactorLoginSettings>();
            var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, twoFactorSettings.NumberOfRecoveryCodesToGenerate);

            await SetRecoveryCodes(recoveryCodes.ToArray(), await UserManager.GetUserIdAsync(user));

            return RedirectToAction(nameof(TwoFactorAuthenticationController.ShowRecoveryCodes), _twoFactorAuthenticationControllerName);
        }

        return RedirectToTwoFactorIndex();
    }

    protected async Task<IList<string>> AvailableProvidersAsync(IUser user)
    {
        var providers = await UserManager.GetValidTwoFactorProvidersAsync(user);

        return providers.Intersect(TwoFactorOptions.Providers).ToList();
    }

    protected IActionResult RedirectToTwoFactorIndex()
        => RedirectToAction(nameof(TwoFactorAuthenticationController.Index), _twoFactorAuthenticationControllerName);

    protected IActionResult RedirectToAccountLogin()
        => RedirectToAction(nameof(AccountController.Login), _accountControllerName);

    protected static string StripToken(string code)
        => code.Replace(" ", String.Empty).Replace("-", String.Empty);

    protected static string GetRecoveryCodesCacheKey(string userId)
        => $"TwoFactorAuthenticationRecoveryCodes_{userId}";
}
