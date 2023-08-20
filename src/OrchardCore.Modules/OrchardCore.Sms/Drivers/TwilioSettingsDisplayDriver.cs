using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Drivers;

public class TwilioSettingsDisplayDriver : SectionDisplayDriver<ISite, TwilioSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    protected readonly IStringLocalizer S;

    public TwilioSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IPhoneFormatValidator phoneFormatValidator,
        IDataProtectionProvider dataProtectionProvider,
        IStringLocalizer<TwilioSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _phoneFormatValidator = phoneFormatValidator;
        _dataProtectionProvider = dataProtectionProvider;
        S = stringLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(TwilioSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        return Initialize<TwilioSettingsViewModel>("TwilioSettings_Edit", model =>
        {
            model.PhoneNumber = settings.PhoneNumber;
            model.AccountSID = settings.AccountSID;
            model.HasAuthToken = !String.IsNullOrEmpty(settings.AuthToken);
        }).Location("Content:5")
        .OnGroup(SmsSettings.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(TwilioSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!context.GroupId.Equals(SmsSettings.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        var model = new TwilioSettingsViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix) && model.DefaultProvider == TwilioSmsProvider.TechnicalName)
        {
            if (String.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Phone number requires a value."]);
            }
            else if (!_phoneFormatValidator.IsValid(model.PhoneNumber))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Please provide a valid phone number."]);
            }

            if (String.IsNullOrWhiteSpace(model.AccountSID))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.AccountSID), S["Account SID requires a value."]);
            }

            if (settings.AuthToken == null && String.IsNullOrWhiteSpace(model.AuthToken))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.AuthToken), S["Auth Token required a value."]);
            }

            settings.PhoneNumber = model.PhoneNumber;
            settings.AccountSID = model.AccountSID;

            if (!String.IsNullOrWhiteSpace(model.AuthToken))
            {
                var protector = _dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName);

                settings.AuthToken = protector.Protect(model.AuthToken);
            }
        }

        return await EditAsync(settings, context);
    }
}
