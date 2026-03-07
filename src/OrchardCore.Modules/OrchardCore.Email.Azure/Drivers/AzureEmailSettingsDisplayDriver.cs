using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Email;
using OrchardCore.Email.Azure;
using OrchardCore.Email.Azure.ViewModels;
using OrchardCore.Email.Services;
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
    private readonly EmailOptions _emailOptions;

    internal readonly IStringLocalizer S;

    public AzureEmailSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IHttpContextAccessor httpContextAccessor,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IEmailAddressValidator emailValidator,
        IOptions<EmailOptions> emailOptions,
        IStringLocalizer<AzureEmailSettingsDisplayDriver> stringLocalizer)
    {
        _shellReleaseManager = shellReleaseManager;
        _httpContextAccessor = httpContextAccessor;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _emailValidator = emailValidator;
        _emailOptions = emailOptions.Value;
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
            model.DefaultSender = settings.DefaultSender;
            model.ConnectionString = settings.ConnectionString;
        }).Location("Content:5#Azure Communication Services")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, AzureEmailSettings settings, UpdateEditorContext context)
    {
        if (!await _authorizationService.AuthorizeAsync(_httpContextAccessor.HttpContext?.User, EmailPermissions.ManageEmailSettings))
        {
            return null;
        }

        if (_emailOptions.DefaultProviderName == "Azure")
        {
            var model = new AzureEmailSettingsViewModel();

            await context.Updater.TryUpdateModelAsync(model, Prefix);

            if (string.IsNullOrEmpty(model.DefaultSender))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultSender), S["The Default Sender is a required field."]);
            }

            if (!_emailValidator.Validate(model.DefaultSender))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.DefaultSender), S["The Default Sender is invalid."]);
            }

            if (string.IsNullOrWhiteSpace(model.ConnectionString))
            {
                context.Updater.ModelState.AddModelError(Prefix, nameof(model.ConnectionString), S["Connection string is required."]);
            }

            model.DefaultSender = settings.DefaultSender;

            if (settings.ConnectionString != model.ConnectionString)
            {
                var protector = _dataProtectionProvider.CreateProtector(AzureEmailOptionsConfiguration.ProtectorName);

                settings.ConnectionString = protector.Protect(model.ConnectionString);
            }

            _shellReleaseManager.RequestRelease();
        }

        return await EditAsync(site, settings, context);
    }
}
