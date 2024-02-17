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
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Azure.ViewModels;
using OrchardCore.Email.Services;
using OrchardCore.Entities;
using OrchardCore.Environment.Shell;
using OrchardCore.Mvc.ModelBinding;
using OrchardCore.Settings;

namespace OrchardCore.Azure.Email.Drivers;

public class AzureEmailSettingsDisplayDriver : SectionDisplayDriver<ISite, AzureEmailSettings>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly IEmailAddressValidator _emailValidator;

    protected IStringLocalizer S;
    protected IHtmlLocalizer H;

    public AzureEmailSettingsDisplayDriver(
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IShellHost shellHost,
        ShellSettings shellSettings,
        IEmailAddressValidator emailValidator,
        IStringLocalizer<AzureEmailSettingsDisplayDriver> stringLocalizer,
        IHtmlLocalizer<AzureEmailSettingsDisplayDriver> htmlLocalizer)
    {
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _emailValidator = emailValidator;
        S = stringLocalizer;
        H = htmlLocalizer;
    }

    public override async Task<IDisplayResult> EditAsync(AzureEmailSettings settings, BuildEditorContext context)
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
            var emailSettings = site.As<EmailSettings>();

            var hasChanges = model.IsEnabled != settings.IsEnabled;

            if (!model.IsEnabled)
            {
                if (hasChanges && emailSettings.DefaultProviderName == AzureEmailProvider.TechnicalName)
                {
                    emailSettings.DefaultProviderName = null;

                    site.Put(emailSettings);
                }

                settings.IsEnabled = false;
            }
            else
            {
                settings.IsEnabled = true;

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
                    // Release the tenant to apply the settings when something changed.
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
        }

        return await EditAsync(settings, context);
    }
}
