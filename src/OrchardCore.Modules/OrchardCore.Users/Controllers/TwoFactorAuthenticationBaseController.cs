using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
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

    protected async Task SetRecoveryCodesAsync(string[] codes, string userId)
    {
        var key = GetRecoveryCodesCacheKey(userId);

        var model = new ShowRecoveryCodesViewModel()
        {
            RecoveryCodes = codes ?? [],
        };

        var data = JsonSerializer.SerializeToUtf8Bytes(model);

        await DistributedCache.SetAsync(key, data,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 0, 5)
            });
    }

    protected async Task<IActionResult> RemoveTwoFactorProviderAsync(IUser user, Func<Task> onSuccessAsync)
    {
        var currentProviders = await GetTwoFactorProvidersAsync(user);

        if (currentProviders.Count == 1)
        {
            if (await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync(user))
            {
                await Notifier.ErrorAsync(H["You cannot remove the only active two-factor method."]);

                return RedirectToTwoFactorIndex();
            }

            var result = await UserManager.SetTwoFactorEnabledAsync(user, false);

            if (result.Succeeded)
            {
                await onSuccessAsync();
                await Notifier.WarningAsync(H["Your two-factor authentication has been disabled."]);
                await SignInManager.RefreshSignInAsync(user);
            }
            else
            {
                await Notifier.ErrorAsync(H["Unable to disable two-factor authentication."]);
            }
        }
        else
        {
            await onSuccessAsync();
        }

        return RedirectToTwoFactorIndex();
    }

    protected async Task EnableTwoFactorAuthenticationAsync(IUser user)
    {
        if (await UserManager.GetTwoFactorEnabledAsync(user))
        {
            await RefreshTwoFactorClaimAsync(user);

            return;
        }

        await UserManager.SetTwoFactorEnabledAsync(user, true);

        if (await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync(user))
        {
            await RefreshTwoFactorClaimAsync(user);
        }
    }

    protected async Task RefreshTwoFactorClaimAsync(IUser user)
    {
        var twoFactorClaim = User.Claims.FirstOrDefault(claim => claim.Type == UserConstants.TwoFactorAuthenticationClaimType);

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
            var twoFactorSettings = await SiteService.GetSettingsAsync<TwoFactorLoginSettings>();
            var recoveryCodes = await UserManager.GenerateNewTwoFactorRecoveryCodesAsync(user, twoFactorSettings.NumberOfRecoveryCodesToGenerate);

            await SetRecoveryCodesAsync(recoveryCodes.ToArray(), await UserManager.GetUserIdAsync(user));

            await Notifier.WarningAsync(H["New recovery codes were generated."]);

            return RedirectToAction(nameof(TwoFactorAuthenticationController.ShowRecoveryCodes), _twoFactorAuthenticationControllerName);
        }

        return RedirectToTwoFactorIndex();
    }

    protected async Task<IList<string>> GetTwoFactorProvidersAsync(IUser user)
    {
        var providers = await UserManager.GetValidTwoFactorProvidersAsync(user);

        return providers.Intersect(TwoFactorOptions.Providers).ToList();
    }

    protected IActionResult RedirectToTwoFactorIndex()
        => RedirectToAction(nameof(TwoFactorAuthenticationController.Index), _twoFactorAuthenticationControllerName);

    protected IActionResult RedirectToAccountLogin()
        => RedirectToAction(nameof(AccountController.Login), _accountControllerName);

    protected IActionResult UserNotFound()
        => NotFound("Unable to load user.");

    protected static string StripToken(string code)
        => code.Replace(" ", string.Empty).Replace("-", string.Empty);

    protected static string GetRecoveryCodesCacheKey(string userId)
        => $"TwoFactorAuthenticationRecoveryCodes_{userId}";
}
