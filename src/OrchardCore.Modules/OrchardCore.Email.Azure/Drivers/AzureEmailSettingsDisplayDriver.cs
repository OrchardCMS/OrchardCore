using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.ModelBinding;
using OrchardCore.DisplayManagement.Notify;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.ViewModels;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;

namespace OrchardCore.Azure.Email.Drivers;

public class AzureEmailSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureEmailSettings>
{
    public const string AzureEmailSettingProtector = "AzureEmailProtector";

    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AzureEmailOptions _azureEmailOptions;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly INotifier _notifier;

    protected IStringLocalizer S;
    protected IHtmlLocalizer H;

    public AzureEmailSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IOptions<AzureEmailOptions> azureEmailOptions,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IShellHost shellHost,
        ShellSettings shellSettings,
        INotifier notifier,
        IStringLocalizer<AzureEmailSettingsDisplayDriver> stringLocalizer,
        IHtmlLocalizer<AzureEmailSettingsDisplayDriver> htmlLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _azureEmailOptions = azureEmailOptions.Value;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _notifier = notifier;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(AzureEmailSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
        {
            return null;
        }

        return Initialize<AzureEmailSettingsViewModel>("AzureEmailSettings_Edit", model =>
        {
            var hasFileConnectionString = !string.IsNullOrWhiteSpace(_azureEmailOptions.ConnectionString);

            if (settings.IsSet)
            {
                model.IsEnabled = settings.IsEnabled;
            }
            else
            {
                model.IsEnabled = hasFileConnectionString && !string.IsNullOrWhiteSpace(_azureEmailOptions.DefaultSender);
            };

            model.DefaultSender = settings.DefaultSender ?? _azureEmailOptions.DefaultSender;
            model.ConfigurationExists = !string.IsNullOrWhiteSpace(settings.ConnectionString);
            model.FileConfigurationExists = hasFileConnectionString;
            model.PreventUIConnectionChange = _azureEmailOptions.PreventUIConnectionChange;

        }).Location("Content:5#Azure")
        .OnGroup(EmailSettings.GroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureEmailSettings settings, IUpdateModel updater, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageEmailSettings))
        {
            return null;
        }

        var model = new AzureEmailSettingsViewModel();

        if (await updater.TryUpdateModelAsync(model, Prefix))
        {
            settings.IsSet = true;

            var emailSettings = site.As<EmailSettings>();

            var hasChanges = model.IsEnabled == settings.IsEnabled;

            if (!model.IsEnabled)
            {
                if (hasChanges && emailSettings.DefaultProviderName == AzureEmailProvider.TechnicalName)
                {
                    emailSettings.DefaultProviderName = null;

                    site.Put(emailSettings);

                    await _notifier.WarningAsync(H["You have successfully disabled the default Email provider. The Email service is now disable and will remain disabled until you designate a new default provider."]);
                }

                settings.IsEnabled = false;
            }
            else
            {
                settings.IsEnabled = true;

                hasChanges |= model.DefaultSender == settings.DefaultSender;

                if (!_azureEmailOptions.PreventUIConnectionChange)
                {
                    if (!string.IsNullOrWhiteSpace(model.ConnectionString) && settings.ConnectionString is null)
                    {
                        context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConnectionString), S["Connection string is required."]);
                    }
                    else if (!string.IsNullOrWhiteSpace(model.ConnectionString))
                    {
                        // Encrypt the connection string.
                        var protector = _dataProtectionProvider.CreateProtector(AzureEmailSettingProtector);

                        var protectedConnection = protector.Protect(model.ConnectionString);

                        // Check if the connection string changed before setting it.
                        hasChanges |= protectedConnection != settings.ConnectionString;

                        settings.ConnectionString = protectedConnection;
                    }
                }
            }

            // Release the tenant to apply the settings when something changed.
            if (context.Updater.ModelState.IsValid && hasChanges)
            {
                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }
        }

        return await EditAsync(settings, context);
    }
}
