using System;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Authorize, Admin, Feature(UserConstants.Features.AuthenticatorApp)]
public class AuthenticatorAppController : TwoFactorAuthenticationBaseController
{
    private const string AuthenticatorUriFormat = "otpauth://totp/{0}:{1}?secret={2}&digits={3}&issuer={0}";

    private readonly UrlEncoder _urlEncoder;
    private readonly ShellSettings _shellSettings;

    public AuthenticatorAppController(
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ISiteService siteService,
        IHtmlLocalizer<AccountController> htmlLocalizer,
        IStringLocalizer<AccountController> stringLocalizer,
        IOptions<TwoFactorOptions> twoFactorOptions,
        INotifier notifier,
        IDistributedCache distributedCache,
        UrlEncoder urlEncoder,
        ShellSettings shellSettings,
        ITwoFactorAuthenticationHandlerCoordinator twoFactorAuthenticationHandlerCoordinator)
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
        _urlEncoder = urlEncoder;
        _shellSettings = shellSettings;
    }

    public async Task<IActionResult> Index(string returnUrl)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var loginSettings = (await SiteService.GetSiteSettingsAsync()).As<AuthenticatorAppLoginSettings>();

        var model = await LoadSharedKeyAndQrCodeUriAsync(user, loginSettings);

        model.ReturnUrl = returnUrl;

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(EnableAuthenticatorViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var isValid = await UserManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, StripToken(model.Code));

        if (!isValid)
        {
            ModelState.AddModelError(model.Code, S["Verification code is invalid."]);

            var loginSettings = (await SiteService.GetSiteSettingsAsync()).As<AuthenticatorAppLoginSettings>();

            return View(await LoadSharedKeyAndQrCodeUriAsync(user, loginSettings));
        }

        await EnableTwoFactorAuthenticationAsync(user);

        await Notifier.SuccessAsync(H["Your authenticator app has been verified."]);

        return await RedirectToTwoFactorAsync(user);
    }

    public async Task<IActionResult> Reset()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var providers = await GetTwoFactorProvidersAsync(user);

        var model = new ResetAuthenticatorViewModel()
        {
            CanRemove = providers.Count > 1 || !await TwoFactorAuthenticationHandlerCoordinator.IsRequiredAsync(),
            WillDisableTwoFactor = providers.Count == 1,
        };

        return View(model);
    }

    [HttpPost, ActionName(nameof(Reset))]
    public async Task<IActionResult> ResetPost()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        return await RemoveTwoFactorProviderAync(user, async () =>
        {
            await UserManager.ResetAuthenticatorKeyAsync(user);

            await Notifier.SuccessAsync(H["Your authenticator app key has been reset."]);
        });
    }

    private async Task<EnableAuthenticatorViewModel> LoadSharedKeyAndQrCodeUriAsync(IUser user, AuthenticatorAppLoginSettings settings)
    {
        // Load the authenticator key & QR code URI to display on the form.
        var unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);

        if (String.IsNullOrEmpty(unformattedKey))
        {
            await UserManager.ResetAuthenticatorKeyAsync(user);
            unformattedKey = await UserManager.GetAuthenticatorKeyAsync(user);
        }

        var displayName = await GetUserDisplayNameAsync(user, settings.UseEmailAsAuthenticatorDisplayName);

        return new EnableAuthenticatorViewModel()
        {
            SharedKey = FormatKey(unformattedKey),
            AuthenticatorUri = await GenerateQrCodeUriAsync(displayName, unformattedKey, settings.TokenLength),
        };
    }

    private Task<string> GetUserDisplayNameAsync(IUser user, bool showEmail)
        => showEmail ? UserManager.GetEmailAsync(user) : UserManager.GetUserNameAsync(user);

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

    private async Task<string> GenerateQrCodeUriAsync(string displayName, string unformattedKey, int tokenLength)
    {
        var site = await SiteService.GetSiteSettingsAsync();

        var issuer = String.IsNullOrWhiteSpace(site.SiteName) ? _shellSettings.Name : site.SiteName.Trim();

        return String.Format(
            CultureInfo.InvariantCulture,
            AuthenticatorUriFormat,
            _urlEncoder.Encode(issuer),
            _urlEncoder.Encode(displayName),
            unformattedKey,
            tokenLength);
    }
}
