using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Azure.ViewModels;
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
    private readonly IEmailAddressValidator _emailValidator;

    internal readonly IStringLocalizer S;

    public AzureEmailSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IEmailAddressValidator emailValidator,
        IStringLocalizer<AzureEmailSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _emailValidator = emailValidator;
        S = stringLocalizer;
    }

    protected override string SettingsGroupId
        => EmailSettings.GroupId;

    public override async Task<IDisplayResult> EditAsync(ISite site, AzureEmailSettings settings, BuildEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        return Initialize<AzureEmailSettingsViewModel>("AzureEmailSettings_Edit", model =>
        {
            model.IsEnabled = settings.IsEnabled;
            model.DefaultSender = settings.DefaultSender;
            model.ConnectionStringSecretName = settings.ConnectionStringSecretName;
            model.HasConnectionString = !string.IsNullOrWhiteSpace(settings.ConnectionString);
        }).Location("Content:5#Azure Communication Services")
        .OnGroup(SettingsGroupId);
#pragma warning restore CS0618 // Type or member is obsolete
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureEmailSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
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

#pragma warning disable CS0618 // Type or member is obsolete
            // Validate that either a secret is selected or legacy connection string exists
            if (string.IsNullOrWhiteSpace(model.ConnectionStringSecretName) && string.IsNullOrWhiteSpace(settings.ConnectionString))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConnectionStringSecretName), S["A connection string secret is required."]);
            }

            hasChanges |= model.ConnectionStringSecretName != settings.ConnectionStringSecretName;
            settings.ConnectionStringSecretName = model.ConnectionStringSecretName;
#pragma warning restore CS0618 // Type or member is obsolete
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
