using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Controllers;

public sealed class AdminController : Controller
{
    private readonly SmsProviderOptions _smsProviderOptions;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly INotifier _notifier;
    private readonly IAuthorizationService _authorizationService;
    private readonly ISmsProviderResolver _smsProviderResolver;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    public AdminController(
        IOptions<SmsProviderOptions> smsProviderOptions,
        IPhoneFormatValidator phoneFormatValidator,
        ISmsProviderResolver smsProviderResolver,
        INotifier notifier,
        IAuthorizationService authorizationService,
        IHtmlLocalizer<AdminController> htmlLocalizer,
        IStringLocalizer<AdminController> stringLocalizer)
    {
        _smsProviderOptions = smsProviderOptions.Value;
        _phoneFormatValidator = phoneFormatValidator;
        _smsProviderResolver = smsProviderResolver;
        _notifier = notifier;
        _authorizationService = authorizationService;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    [Admin("sms/test", "SmsProviderTest")]
    public async Task<IActionResult> Test()
    {
        if (!await _authorizationService.AuthorizeAsync(User, SmsPermissions.ManageSmsSettings))
        {
            return Forbid();
        }

        var model = new SmsTestViewModel();

        PopulateModel(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Test(SmsTestViewModel model)
    {
        if (!await _authorizationService.AuthorizeAsync(User, SmsPermissions.ManageSmsSettings))
        {
            return Forbid();
        }

        if (ModelState.IsValid)
        {
            var provider = await _smsProviderResolver.GetAsync(model.Provider);

            if (provider is null)
            {
                ModelState.AddModelError(nameof(model.Provider), S["Please select a valid provider."]);
            }
            else if (!_phoneFormatValidator.IsValid(model.PhoneNumber))
            {
                ModelState.AddModelError(nameof(model.PhoneNumber), S["Please provide a valid phone number."]);
            }
            else
            {
                var result = await provider.SendAsync(new SmsMessage()
                {
                    To = model.PhoneNumber,
                    Body = S["This is a test SMS message."]
                });

                if (result.Succeeded)
                {
                    await _notifier.SuccessAsync(H["The test SMS message has been successfully sent."]);

                    return RedirectToAction(nameof(Test));
                }
                else
                {
                    await _notifier.ErrorAsync(H["The test SMS message failed to send."]);
                }
            }
        }

        PopulateModel(model);

        return View(model);
    }

    private void PopulateModel(SmsTestViewModel model)
    {
        model.Providers = _smsProviderOptions.Providers
            .Where(entry => entry.Value.IsEnabled)
            .Select(entry => new SelectListItem(entry.Key, entry.Key))
            .OrderBy(item => item.Text)
            .ToArray();
    }
}
