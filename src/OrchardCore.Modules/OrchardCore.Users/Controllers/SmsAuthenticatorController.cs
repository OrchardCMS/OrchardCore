using System.Text;
using System.Text.Encodings.Web;
using Fluid.Values;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Authorize]
[Feature(UserConstants.Features.SmsAuthenticator)]
public sealed class SmsAuthenticatorController : TwoFactorAuthenticationBaseController
{
    private readonly IdentityOptions _identityOptions;
    private readonly IUserService _userService;
    private readonly ISmsService _smsService;
    private readonly ILiquidTemplateManager _liquidTemplateManager;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly HtmlEncoder _htmlEncoder;

    public SmsAuthenticatorController(
        SignInManager<IUser> signInManager,
        UserManager<IUser> userManager,
        ISiteService siteService,
        IHtmlLocalizer<AccountController> htmlLocalizer,
        IStringLocalizer<AccountController> stringLocalizer,
        IOptions<TwoFactorOptions> twoFactorOptions,
        IOptions<IdentityOptions> identityOptions,
        INotifier notifier,
        IDistributedCache distributedCache,
        IUserService userService,
        ISmsService smsService,
        ILiquidTemplateManager liquidTemplateManager,
        IPhoneFormatValidator phoneFormatValidator,
        HtmlEncoder htmlEncoder,
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
        _identityOptions = identityOptions.Value;
        _userService = userService;
        _smsService = smsService;
        _liquidTemplateManager = liquidTemplateManager;
        _phoneFormatValidator = phoneFormatValidator;
        _htmlEncoder = htmlEncoder;
    }

    public async Task<IActionResult> Index()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var settings = await SiteService.GetSettingsAsync<LoginSettings>();

        var currentPhoneNumber = await UserManager.GetPhoneNumberAsync(user);

        var model = new RequestCodeSmsAuthenticatorViewModel()
        {
            PhoneNumber = currentPhoneNumber,
            AllowChangingPhoneNumber = settings.AllowChangingPhoneNumber
            || string.IsNullOrEmpty(currentPhoneNumber)
            || !_phoneFormatValidator.IsValid(currentPhoneNumber),
        };

        return View(model);
    }

    [HttpPost]
    [ActionName(nameof(Index))]
    public async Task<IActionResult> IndexPost(RequestCodeSmsAuthenticatorViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var settings = await SiteService.GetSettingsAsync<LoginSettings>();

        var currentPhoneNumber = await UserManager.GetPhoneNumberAsync(user);

        var canSetNewPhone = settings.AllowChangingPhoneNumber ||
            string.IsNullOrEmpty(currentPhoneNumber) ||
            !_phoneFormatValidator.IsValid(currentPhoneNumber);

        model.AllowChangingPhoneNumber = canSetNewPhone;

        if (canSetNewPhone && !_phoneFormatValidator.IsValid(model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), S["Invalid phone number used."]);

            return View(model);
        }

        var phoneNumber = canSetNewPhone ? model.PhoneNumber : currentPhoneNumber;

        var code = await UserManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
        var smsSettings = await SiteService.GetSettingsAsync<SmsAuthenticatorLoginSettings>();

        var message = new SmsMessage()
        {
            To = phoneNumber,
            Body = await GetBodyAsync(smsSettings, user, code),
        };

        var result = await _smsService.SendAsync(message);

        if (!result.Succeeded)
        {
            await Notifier.ErrorAsync(H["We are unable to send you an SMS message at this time. Please try again later."]);

            return RedirectToAction(nameof(Index));
        }

        await Notifier.SuccessAsync(H["We have successfully sent an verification code to your phone number. Please retrieve the code from your device."]);

        await SetPendingPhoneNumberAsync(phoneNumber, user.UserName);

        return RedirectToAction(nameof(ValidateCode));
    }

    public async Task<IActionResult> ValidateCode()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var pendingPhoneNumber = await GetPendingPhoneNumberAsync(user.UserName);

        if (pendingPhoneNumber == null)
        {
            await Notifier.ErrorAsync(H["Unable to find a phone number."]);

            return RedirectToAction(nameof(Index));
        }

        var model = new EnableSmsAuthenticatorViewModel
        {
            PhoneNumber = pendingPhoneNumber,
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> ValidateCode(EnableSmsAuthenticatorViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        if (string.IsNullOrWhiteSpace(model.PhoneNumber))
        {
            await Notifier.ErrorAsync(H["Unable to find a phone number."]);

            return RedirectToAction(nameof(Index));
        }

        if (ModelState.IsValid)
        {
            var result = await UserManager.ChangePhoneNumberAsync(user, model.PhoneNumber, StripToken(model.Code));
            if (result.Succeeded)
            {
                await EnableTwoFactorAuthenticationAsync(user);

                await Notifier.SuccessAsync(H["Your phone number has been confirmed."]);

                return await RedirectToTwoFactorAsync(user);
            }

            await Notifier.ErrorAsync(H["Invalid verification code."]);
        }

        return View(model);
    }

    private Task<string> GetBodyAsync(SmsAuthenticatorLoginSettings settings, IUser user, string code)
    {
        var message = string.IsNullOrWhiteSpace(settings.Body)
        ? EmailAuthenticatorLoginSettings.DefaultBody
        : settings.Body;

        return GetContentAsync(message, user, code);
    }

    private async Task<string> GetContentAsync(string message, IUser user, string code)
    {
        var result = await _liquidTemplateManager.RenderHtmlContentAsync(message, _htmlEncoder, null,
            new Dictionary<string, FluidValue>()
            {
                ["User"] = new ObjectValue(user),
                ["Code"] = new StringValue(code),
            });

        using var writer = new StringWriter();
        result.WriteTo(writer, _htmlEncoder);

        return writer.ToString();
    }

    private async Task SetPendingPhoneNumberAsync(string phoneNumber, string username)
    {
        var key = GetPhoneChangeCacheKey(username);

        var data = Encoding.UTF8.GetBytes(phoneNumber);

        await DistributedCache.SetAsync(key, data,
            new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 10, 0)
            });
    }

    private async Task<string> GetPendingPhoneNumberAsync(string username)
    {
        var key = GetPhoneChangeCacheKey(username);

        var data = await DistributedCache.GetAsync(key);

        if (data != null && data.Length > 0)
        {
            return Encoding.UTF8.GetString(data);
        }

        return null;
    }

    private static string GetPhoneChangeCacheKey(string username)
        => $"TwoFactorAuthenticationPhoneNumberChange_{username}";
}
