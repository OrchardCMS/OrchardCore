using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;
using OrchardCore.Sms.Azure.Services;
using OrchardCore.Sms.Azure.ViewModels;

namespace OrchardCore.Sms.Azure.Drivers;

public class AzureSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly INotifier _notifier;

    protected readonly IHtmlLocalizer H;
    protected readonly IStringLocalizer S;

    public AzureSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IPhoneFormatValidator phoneFormatValidator,
        IDataProtectionProvider dataProtectionProvider,
        IShellHost shellHost,
        ShellSettings shellSettings,
        INotifier notifier,
        IHtmlLocalizer<AzureSettingsDisplayDriver> htmlLocalizer,
        IStringLocalizer<AzureSettingsDisplayDriver> stringLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _phoneFormatValidator = phoneFormatValidator;
        _dataProtectionProvider = dataProtectionProvider;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _notifier = notifier;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(AzureSettings settings)
    {
        return Initialize<AzureSettingsViewModel>("TwilioSettings_Edit", model =>
        {
            model.IsEnabled = settings.IsEnabled;
            model.ConnectionString = settings.ConnectionString;
            model.PhoneNumber = settings.PhoneNumber;
        }).Location("Content:5#Twilio")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings))
        .OnGroup(SmsSettings.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureSettings settings, IUpdateModel updater, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!context.GroupId.Equals(SmsSettings.GroupId, StringComparison.OrdinalIgnoreCase)
            || !await _authorizationService.AuthorizeAsync(user, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        var model = new AzureSettingsViewModel();

        if (await context.Updater.TryUpdateModelAsync(model, Prefix))
        {
            var hasChanges = settings.IsEnabled != model.IsEnabled;

            if (!model.IsEnabled)
            {
                var smsSettings = site.As<SmsSettings>();

                if (hasChanges && smsSettings.DefaultProviderName == AzureSmsProvider.TechnicalName)
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

                if (string.IsNullOrWhiteSpace(model.ConnectionString))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConnectionString), S["ConnectionString requires a value."]);
                }

                if (string.IsNullOrWhiteSpace(model.PhoneNumber))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Phone number requires a value."]);
                }
                else if (!_phoneFormatValidator.IsValid(model.PhoneNumber))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Please provide a valid phone number."]);
                }

                // Has change should be evaluated before updating the value.
                hasChanges |= settings.ConnectionString != model.ConnectionString;
                hasChanges |= settings.PhoneNumber != model.PhoneNumber;

                settings.ConnectionString = model.ConnectionString;
                settings.PhoneNumber = model.PhoneNumber;
            }

            if (context.Updater.ModelState.IsValid && hasChanges)
            {
                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }
        }

        return Edit(settings);
    }
}
