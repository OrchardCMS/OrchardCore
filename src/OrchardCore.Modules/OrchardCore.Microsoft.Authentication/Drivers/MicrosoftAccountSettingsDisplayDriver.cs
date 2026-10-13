using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Microsoft.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Microsoft.Authentication.Drivers;

public sealed class MicrosoftAccountSettingsDisplayDriver : SiteDisplayDriver<MicrosoftAccountSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public MicrosoftAccountSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<MicrosoftAccountSettingsDisplayDriver> logger)
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override string SettingsGroupId
        => MicrosoftAuthenticationConstants.Features.MicrosoftAccount;

    public override async Task<IDisplayResult> EditAsync(ISite site, MicrosoftAccountSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
        {
            return null;
        }

        return Initialize<MicrosoftAccountSettingsViewModel>("MicrosoftAccountSettings_Edit", model =>
        {
            model.AppId = settings.AppId;
            if (!string.IsNullOrWhiteSpace(settings.AppSecret))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);
                    model.AppSecret = protector.Unprotect(settings.AppSecret);
                }
                catch (CryptographicException)
                {
                    _logger.LogError("The app secret could not be decrypted. It may have been encrypted using a different key.");
                    model.AppSecret = string.Empty;
                    model.HasDecryptionError = true;
                }
            }
            else
            {
                model.AppSecret = string.Empty;
            }
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath.Value;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, MicrosoftAccountSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageMicrosoftAuthentication))
        {
            return null;
        }

        var model = new MicrosoftAccountSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.AppId = model.AppId;
        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        if (context.Updater.ModelState.IsValid)
        {
            var protector = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount);

            settings.AppSecret = protector.Protect(model.AppSecret);

            // Refresh the options built from these settings, so that they apply without restarting the tenant.
            _optionsUpdateNotifier.RequestMicrosoftAccountUpdate();
        }

        return await EditAsync(site, settings, context);
    }
}
