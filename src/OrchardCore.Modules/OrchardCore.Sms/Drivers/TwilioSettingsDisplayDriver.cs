using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Sms.Models;
using OrchardCore.Sms.Services;
using OrchardCore.Sms.ViewModels;

namespace OrchardCore.Sms.Drivers;

public sealed class TwilioSettingsDisplayDriver : SiteDisplayDriver<TwilioSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly INotifier _notifier;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => SmsSettings.GroupId;

    public TwilioSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IPhoneFormatValidator phoneFormatValidator,
        IDataProtectionProvider dataProtectionProvider,
        INotifier notifier,
        IHtmlLocalizer<TwilioSettingsDisplayDriver> htmlLocalizer,
        IStringLocalizer<TwilioSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _phoneFormatValidator = phoneFormatValidator;
        _dataProtectionProvider = dataProtectionProvider;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, TwilioSettings settings, BuildEditorContext c)
    {
        return Initialize<TwilioSettingsViewModel>("TwilioSettings_Edit", model =>
        {
            model.IsEnabled = settings.IsEnabled;
            model.PhoneNumber = settings.PhoneNumber;
            model.AccountSID = settings.AccountSID;
            model.HasAuthToken = !string.IsNullOrEmpty(settings.AuthToken);
        }).Location("Content:5#Twilio")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, TwilioSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        var model = new TwilioSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);
        var hasChanges = settings.IsEnabled != model.IsEnabled;
        var smsSettings = site.As<SmsSettings>();

        if (!model.IsEnabled)
        {
            if (hasChanges && smsSettings.DefaultProviderName == TwilioSmsProvider.TechnicalName)
            {
                await _notifier.WarningAsync(H["You have successfully disabled the default SMS provider. The SMS service is now disable and will remain disabled until you designate a new default provider."]);

                smsSettings.DefaultProviderName = null;

                site.Put(smsSettings);
            }

            settings.IsEnabled = false;
        }
        else
        {
            settings.IsEnabled = true;

            if (string.IsNullOrWhiteSpace(model.PhoneNumber))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Phone number requires a value."]);
            }
            else if (!_phoneFormatValidator.IsValid(model.PhoneNumber))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Please provide a valid phone number."]);
            }

            if (string.IsNullOrWhiteSpace(model.AccountSID))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.AccountSID), S["Account SID requires a value."]);
            }

            if (settings.AuthToken == null && string.IsNullOrWhiteSpace(model.AuthToken))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.AuthToken), S["Auth Token required a value."]);
            }

            // Has change should be evaluated before updating the value.
            hasChanges |= settings.PhoneNumber != model.PhoneNumber;
            hasChanges |= settings.AccountSID != model.AccountSID;

            settings.PhoneNumber = model.PhoneNumber;
            settings.AccountSID = model.AccountSID;

            if (!string.IsNullOrWhiteSpace(model.AuthToken))
            {
                var protector = _dataProtectionProvider.CreateProtector(TwilioSmsProvider.ProtectorName);

                var protectedToken = protector.Protect(model.AuthToken);
                hasChanges |= settings.AuthToken != protectedToken;

                settings.AuthToken = protectedToken;
            }
        }

        if (context.Updater.ModelState.IsValid && settings.IsEnabled && string.IsNullOrEmpty(smsSettings.DefaultProviderName))
        {
            // If we are enabling the only provider, set it as the default one.
            smsSettings.DefaultProviderName = TwilioSmsProvider.TechnicalName;

            site.Put(smsSettings);

            hasChanges = true;
        }

        if (hasChanges)
        {
            _shellReleaseManager.RequestRelease();
        }

        return Edit(site, settings, context);
    }
}
