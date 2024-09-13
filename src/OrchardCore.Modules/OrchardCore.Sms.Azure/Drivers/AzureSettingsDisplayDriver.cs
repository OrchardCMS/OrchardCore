using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
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

    internal readonly IHtmlLocalizer H;
    internal readonly IStringLocalizer S;

    protected override string SettingsGroupId => SmsSettings.GroupId;

    public AzureSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IPhoneFormatValidator phoneFormatValidator,
        IDataProtectionProvider dataProtectionProvider,
        IHtmlLocalizer<AzureSettingsDisplayDriver> htmlLocalizer,
        IStringLocalizer<AzureSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _phoneFormatValidator = phoneFormatValidator;
        _dataProtectionProvider = dataProtectionProvider;
        H = htmlLocalizer;
        S = stringLocalizer;
    }

    public override IDisplayResult Edit(ISite site, AzureSmsSettings settings, BuildEditorContext c)
    {
        return Initialize<AzureSettingsViewModel>("AzureSettings_Edit", model =>
        {
            model.IsEnabled = settings.IsEnabled;
            model.PhoneNumber = settings.PhoneNumber;
            model.ConnectionString = settings.ConnectionString;
        }).Location("Content:5#Azure")
        .RenderWhen(() => _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, AzureSmsPermissions.ManageAzureSmsSettings))
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureSmsSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, AzureSmsPermissions.ManageAzureSmsSettings))
        {
            return null;
        }

        var model = new AzureSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var hasChanges = settings.IsEnabled != model.IsEnabled;
        if (!model.IsEnabled)
        {
            var smsSettings = site.As<SmsSettings>();

            if (hasChanges && smsSettings.DefaultProviderName == AzureSmsProvider.TechnicalName)
            {
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
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Invalid Phone number."]);
            }
            else if (!_phoneFormatValidator.IsValid(model.PhoneNumber))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.PhoneNumber), S["Invalid phone number."]);
            }

            if (string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConnectionString), S["The ConnectionString is required."]);
            }

            // Has change should be evaluated before updating the value.
            hasChanges |= settings.PhoneNumber != model.PhoneNumber;
            hasChanges |= settings.ConnectionString != model.ConnectionString;

            settings.PhoneNumber = model.PhoneNumber;
            settings.ConnectionString = model.ConnectionString;

            if (!string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                var protector = _dataProtectionProvider.CreateProtector(AzureSmsProvider.ProtectorName);

                var protectedToken = protector.Protect(model.ConnectionString);

                hasChanges |= settings.ConnectionString != protectedToken;

                settings.ConnectionString = protectedToken;
            }
        }

        if (hasChanges)
        {
            _shellReleaseManager.RequestRelease();
        }

        return Edit(site, settings, context);
    }
}
