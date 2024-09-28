using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Azure.ViewModels;
using OrchardCore.Email.Core;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;

namespace OrchardCore.Azure.Email.Drivers;

public sealed class AzureEmailSettingsDisplayDriver : SiteDisplayDriver<AzureEmailSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IEmailAddressValidator _emailValidator;

    internal IStringLocalizer S;

    public AzureEmailSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IEmailAddressValidator emailValidator,
        IStringLocalizer<AzureEmailSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _emailValidator = emailValidator;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, AzureEmailSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageEmailSettings))
        {
            return null;
        }

        return Initialize<AzureEmailSettingsViewModel>("AzureEmailSettings_Edit", model =>
        {
            model.IsEnabled = settings.IsEnabled;
            model.DefaultSender = settings.DefaultSender;
            model.HasConnectionString = !string.IsNullOrWhiteSpace(settings.ConnectionString);
        }).Location("Content:5#Azure Communication Services")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureEmailSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, Permissions.ManageEmailSettings))
        {
            return null;
        }

        var model = new AzureEmailSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var emailSettings = site.As<EmailSettings>();

        var hasChanges = model.IsEnabled != settings.IsEnabled;

        settings.IsEnabled = model.IsEnabled;

        if (!model.IsEnabled)
        {
            if (hasChanges && emailSettings.DefaultProviderName == AzureEmailProvider.TechnicalName)
            {
                emailSettings.DefaultProviderName = null;

                site.Put(emailSettings);
            }
        }
        else
        {
            hasChanges |= model.DefaultSender != settings.DefaultSender;

            if (string.IsNullOrEmpty(model.DefaultSender))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultSender), S["The Default Sender is a required field."]);
            }
            else if (!_emailValidator.Validate(model.DefaultSender))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultSender), S["The Default Sender is invalid."]);
            }

            settings.DefaultSender = model.DefaultSender;

            if (string.IsNullOrWhiteSpace(model.ConnectionString)
                && settings.ConnectionString is null)
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConnectionString), S["Connection string is required."]);
            }
            else if (!string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                // Encrypt the connection string.
                var protector = _dataProtectionProvider.CreateProtector(AzureEmailOptionsConfiguration.ProtectorName);

                var protectedConnection = protector.Protect(model.ConnectionString);

                // Check if the connection string changed before setting it.
                hasChanges |= protectedConnection != settings.ConnectionString;

                settings.ConnectionString = protectedConnection;
            }
        }

        if (context.Updater.ModelState.IsValid)
        {
            if (settings.IsEnabled && string.IsNullOrEmpty(emailSettings.DefaultProviderName))
            {
                // If we are enabling the only provider, set it as the default one.
                emailSettings.DefaultProviderName = AzureEmailProvider.TechnicalName;
                site.Put(emailSettings);

                hasChanges = true;
            }

            if (hasChanges)
            {
                _shellReleaseManager.RequestRelease();
            }
        }

        return await EditAsync(site, settings, context);
    }
}
