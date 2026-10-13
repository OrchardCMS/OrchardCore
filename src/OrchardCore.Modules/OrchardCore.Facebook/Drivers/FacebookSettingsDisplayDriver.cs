using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Options;
using OrchardCore.Facebook.Settings;
using OrchardCore.Facebook.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Facebook.Drivers;

public sealed class FacebookSettingsDisplayDriver : SiteDisplayDriver<FacebookSettings>
{
    private readonly IOptionsUpdateNotifier _optionsUpdateNotifier;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public FacebookSettingsDisplayDriver(
        IOptionsUpdateNotifier optionsUpdateNotifier,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<FacebookSettingsDisplayDriver> logger
        )
    {
        _optionsUpdateNotifier = optionsUpdateNotifier;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override string SettingsGroupId
        => FacebookConstants.Features.Core;

    public override async Task<IDisplayResult> EditAsync(ISite site, FacebookSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

        return Initialize<FacebookSettingsViewModel>("FacebookSettings_Edit", model =>
        {
            var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core);

            model.AppId = settings.AppId;
            model.FBInit = settings.FBInit;
            model.FBInitParams = settings.FBInitParams;
            model.Version = settings.Version;
            model.SdkJs = settings.SdkJs;
            if (!string.IsNullOrWhiteSpace(settings.AppSecret))
            {
                try
                {
                    model.AppSecret = protector.Unprotect(settings.AppSecret);
                }
                catch (CryptographicException)
                {
                    _logger.LogError("The app secret could not be decrypted. It may have been encrypted using a different key.");
                    model.AppSecret = string.Empty;
                    model.HasDecryptionError = true;
                }
            }
        }).Location("Content:0")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, FacebookSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageFacebookApp))
        {
            return null;
        }

        var model = new FacebookSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.AppId = model.AppId;
        settings.FBInit = model.FBInit;
        settings.SdkJs = model.SdkJs;
        settings.Version = model.Version;

        if (!string.IsNullOrWhiteSpace(model.FBInitParams))
        {
            settings.FBInitParams = model.FBInitParams;
        }

        if (context.Updater.ModelState.IsValid)
        {
            var protector = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core);
            settings.AppSecret = protector.Protect(model.AppSecret);

            // Refresh the options built from these settings, so that they apply without restarting the tenant.
            _optionsUpdateNotifier.RequestFacebookUpdate();
        }

        return await EditAsync(site, settings, context);
    }
}
