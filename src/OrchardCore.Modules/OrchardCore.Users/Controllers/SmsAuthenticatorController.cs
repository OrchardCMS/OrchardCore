using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Fluid.Values;
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
using OrchardCore.Liquid;
using OrchardCore.Modules;
using OrchardCore.Settings;
using OrchardCore.Sms;
using OrchardCore.Users.Events;
using OrchardCore.Users.Models;
using OrchardCore.Users.Services;
using OrchardCore.Users.ViewModels;

namespace OrchardCore.Users.Controllers;

[Authorize, Feature(UserConstants.Features.SmsAuthenticator)]
public class SmsAuthenticatorController : TwoFactorAuthenticationBaseController
{
    private readonly IUserService _userService;
    private readonly ISmsProvider _smsProvider;
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
        INotifier notifier,
        IDistributedCache distributedCache,
        IUserService userService,
        ISmsProvider smsProvider,
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
        _userService = userService;
        _smsProvider = smsProvider;
        _liquidTemplateManager = liquidTemplateManager;
        _phoneFormatValidator = phoneFormatValidator;
        _htmlEncoder = htmlEncoder;
    }

    [Admin]
    public async Task<IActionResult> Index()
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var settings = (await SiteService.GetSiteSettingsAsync()).As<LoginSettings>();

        var currentPhoneNumber = await UserManager.GetPhoneNumberAsync(user);

        var model = new RequestCodeSmsAuthenticatorViewModel()
        {
            PhoneNumber = currentPhoneNumber,
            AllowChangingPhoneNumber = settings.AllowChangingPhoneNumber
            || String.IsNullOrEmpty(currentPhoneNumber)
            || !_phoneFormatValidator.IsValid(currentPhoneNumber),
        };

        return View(model);
    }

    [HttpPost, Admin, ActionName(nameof(Index))]
    public async Task<IActionResult> IndexPost(RequestCodeSmsAuthenticatorViewModel model)
    {
        var user = await UserManager.GetUserAsync(User);
        if (user == null)
        {
            return UserNotFound();
        }

        var settings = (await SiteService.GetSiteSettingsAsync()).As<LoginSettings>();

        var currentPhoneNumber = await UserManager.GetPhoneNumberAsync(user);

        var canSetNewPhone = settings.AllowChangingPhoneNumber
            || String.IsNullOrEmpty(currentPhoneNumber)
            || !_phoneFormatValidator.IsValid(currentPhoneNumber);

        model.AllowChangingPhoneNumber = canSetNewPhone;

        if (canSetNewPhone && !_phoneFormatValidator.IsValid(model.PhoneNumber))
        {
            ModelState.AddModelError(nameof(model.PhoneNumber), S["Invalid phone number used."]);

            return View(model);
        }

        var phoneNumber = canSetNewPhone ? model.PhoneNumber : currentPhoneNumber;

        var code = await UserManager.GenerateChangePhoneNumberTokenAsync(user, phoneNumber);
        var smsSettings = (await SiteService.GetSiteSettingsAsync()).As<SmsAuthenticatorLoginSettings>();

        var message = new SmsMessage()
        {
            To = phoneNumber,
            Body = await GetBodyAsync(smsSettings, user, code),
        };

        var result = await _smsProvider.SendAsync(message);

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

        if (String.IsNullOrWhiteSpace(model.PhoneNumber))
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

    [HttpPost, Produces("application/json"), AllowAnonymous]
    public async Task<IActionResult> SendCode()
    {
        var user = await SignInManager.GetTwoFactorAuthenticationUserAsync();
        var errorMessage = S["The SMS message could not be sent. Please attempt to request the code at a later time."];

        if (user == null)
        {
            return BadRequest(new
            {
                success = false,
                message = errorMessage.Value,
            });
        }

        var settings = (await SiteService.GetSiteSettingsAsync()).As<SmsAuthenticatorLoginSettings>();
        var code = await UserManager.GenerateTwoFactorTokenAsync(user, TokenOptions.DefaultPhoneProvider);

        var message = new SmsMessage()
        {
            To = await UserManager.GetPhoneNumberAsync(user),
            Body = await GetBodyAsync(settings, user, code),
        };

        var result = await _smsProvider.SendAsync(message);

        return Ok(new
        {
            success = result.Succeeded,
            message = result.Succeeded ? S["A verification code has been sent to your phone number. Please check your device for the code."].Value
            : errorMessage.Value,
        });
    }

    private Task<string> GetBodyAsync(SmsAuthenticatorLoginSettings settings, IUser user, string code)
    {
        var message = String.IsNullOrWhiteSpace(settings.Body)
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

    protected async Task SetPendingPhoneNumberAsync(string phoneNumber, string username)
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
