using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Secrets;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Models;
using OrchardCore.Sms.Azure.Services;
using OrchardCore.Sms.Azure.ViewModels;

namespace OrchardCore.Sms.Azure.Drivers;

public sealed class AzureSettingsDisplayDriver : SiteDisplayDriver<AzureSmsSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IPhoneFormatValidator _phoneFormatValidator;
    private readonly INotifier _notifier;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId
        => SmsSettings.GroupId;

    public AzureSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IPhoneFormatValidator phoneFormatValidator,
        INotifier notifier,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        IHtmlLocalizer<AzureSettingsDisplayDriver> htmlLocalizer,
        IStringLocalizer<AzureSettingsDisplayDriver> stringLocalizer)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _phoneFormatValidator = phoneFormatValidator;
        _notifier = notifier;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, AzureSmsSettings settings, BuildEditorContext c)
    {
        return Initialize<AzureSettingsViewModel>("AzureSmsSettings_Edit", model =>
        {
            model.IsEnabled = settings.IsEnabled;
            model.PhoneNumber = settings.PhoneNumber;
            model.ConnectionString = SecretInputViewModel.Create(settings.ConnectionString, settings.ConnectionStringSecretName);
        }).Location("Content:5#Azure Communication Services")
        .RenderWhen(static (driver) => driver._authorizationService.AuthorizeAsync(driver._httpContextAccessor.HttpContext?.User, SmsPermissions.ManageSmsSettings), this)
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

        var smsSettings = site.GetOrCreate<SmsSettings>();

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

            var connectionString = await model.ConnectionString.UpdateAsync(new SecretInputUpdateContext(
                _serviceProvider,
                _dataProtectionProvider.CreateProtector(AzureSmsOptionsConfiguration.ProtectorName),
                context.Updater.ModelState,
                $"{Prefix}.{nameof(model.ConnectionString)}")
            {
                ProtectedValue = settings.ConnectionString,
                SecretName = settings.ConnectionStringSecretName,
            });

            if (connectionString.Succeeded)
            {
                if (string.IsNullOrEmpty(connectionString.ProtectedValue) && string.IsNullOrEmpty(connectionString.SecretName))
                {
                    context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConnectionString), S["Connection string is required."]);
                }
                else
                {
                    hasChanges |= connectionString.ProtectedValue != settings.ConnectionString ||
                        connectionString.SecretName != settings.ConnectionStringSecretName;

                    settings.ConnectionString = connectionString.ProtectedValue;
                    settings.ConnectionStringSecretName = connectionString.SecretName;
                }
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
            _optionsUpdateNotifier
                .RequestUpdate<AzureSmsOptions>()
                .RequestUpdate<SmsProviderOptions>();
        }

        return Edit(site, settings, context);
    }
}
