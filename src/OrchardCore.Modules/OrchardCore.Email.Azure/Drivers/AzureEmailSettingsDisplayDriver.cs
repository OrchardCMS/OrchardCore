using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Azure.ViewModels;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Options;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.Azure.Email.Drivers;

public sealed class AzureEmailSettingsDisplayDriver : SiteDisplayDriver<AzureEmailSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IEmailAddressValidator _emailValidator;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IServiceProvider _serviceProvider;

    internal readonly IStringLocalizer S;

    public AzureEmailSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IEmailAddressValidator emailValidator,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        IStringLocalizer<AzureEmailSettingsDisplayDriver> stringLocalizer)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _emailValidator = emailValidator;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
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

        return Initialize<AzureEmailSettingsViewModel>("AzureEmailSettings_Edit", model =>
        {
            model.IsEnabled = settings.IsEnabled;
            model.DefaultSender = settings.DefaultSender;
            model.ConnectionString = SecretInputViewModel.Create(settings.ConnectionString, settings.ConnectionStringSecretName);
        }).Location("Content:5#Azure Communication Services")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureEmailSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        var model = new AzureEmailSettingsViewModel();

        await context.Updater.TryUpdateModelAsync(model, Prefix);

        var emailSettings = site.GetOrCreate<EmailSettings>();

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

            var connectionString = await model.ConnectionString.UpdateAsync(new SecretInputUpdateContext(
                _serviceProvider,
                _dataProtectionProvider.CreateProtector(AzureEmailOptionsConfiguration.ProtectorName),
                context.Updater.ModelState,
                $"{Prefix}.{nameof(model.ConnectionString)}")
            {
                ProtectedValue = settings.ConnectionString,
                SecretName = settings.ConnectionStringSecretName,
                Description = "Azure Communication Services email connection string",
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
                _optionsUpdateNotifier
                    .RequestUpdate<AzureEmailOptions>()
                    .RequestUpdate<EmailProviderOptions>()
                    .RequestUpdate<EmailOptions>();
            }
        }

        return await EditAsync(site, settings, context);
    }
}
