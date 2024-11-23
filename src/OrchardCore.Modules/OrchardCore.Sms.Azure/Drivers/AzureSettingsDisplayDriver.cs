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
using OrchardCore.Sms.Azure.Models;
using OrchardCore.Sms.Azure.Services;
using OrchardCore.Sms.Azure.ViewModels;

namespace OrchardCore.Sms.Azure.Drivers;

public sealed class AzureSettingsDisplayDriver : SiteDisplayDriver<AzureSmsSettings>
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

    public AzureSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IPhoneFormatValidator phoneFormatValidator,
        IDataProtectionProvider dataProtectionProvider,
        INotifier notifier,
        IHtmlLocalizer<AzureSettingsDisplayDriver> htmlLocalizer,
        IStringLocalizer<AzureSettingsDisplayDriver> stringLocalizer)
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

    public override IDisplayResult Edit(ISite site, AzureSmsSettings settings, BuildEditorContext c)
    {
        return Initialize<AzureSettingsViewModel>("AzureSmsSettings_Edit", model =>
        {
            model.IsEnabled = settings.IsEnabled;
            model.PhoneNumber = settings.PhoneNumber;
            model.HasConnectionString = !string.IsNullOrEmpty(settings.ConnectionString);
        }).Location("Content:5#Azure Communication Services")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureSmsSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext.User, SmsPermissions.ManageSmsSettings))
        {
            return null;
        }

        var model = new AzureSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var smsSettings = site.As<SmsSettings>();

        var hasChanges = settings.IsEnabled != model.IsEnabled;
        if (!model.IsEnabled)
        {
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

            hasChanges |= model.PhoneNumber != settings.PhoneNumber;

            if (string.IsNullOrEmpty(model.PhoneNumber))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["The phone number is a required."]);
            }
            else if (!_phoneFormatValidator.IsValid(model.PhoneNumber))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Invalid phone number."]);
            }

            settings.PhoneNumber = model.PhoneNumber;

            if (string.IsNullOrWhiteSpace(model.ConnectionString) && settings.ConnectionString is null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConnectionString), S["Connection string is required."]);
            }
            else if (!string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                var protector = _dataProtectionProvider.CreateProtector(AzureSmsOptionsConfiguration.ProtectorName);

                var protectedConnection = protector.Protect(model.ConnectionString);

                // Check if the connection string changed before setting it.
                hasChanges |= protectedConnection != settings.ConnectionString;

                settings.ConnectionString = protectedConnection;
            }
        }

        if (context.Updater.ModelState.IsValid && settings.IsEnabled && string.IsNullOrEmpty(smsSettings.DefaultProviderName))
        {
            // If we are enabling the only provider, set it as the default one.
            smsSettings.DefaultProviderName = AzureSmsProvider.TechnicalName;
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
