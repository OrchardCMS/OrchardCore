using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Google.Authentication.ViewModels;
using OrchardCore.Settings;

namespace OrchardCore.Google.Authentication.Drivers;

public sealed class GoogleAuthenticationSettingsDisplayDriver : SiteDisplayDriver<GoogleAuthenticationSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public GoogleAuthenticationSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<GoogleAuthenticationSettingsDisplayDriver> logger)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override string SettingsGroupId
        => GoogleConstants.Features.GoogleAuthentication;

    public override async Task<IDisplayResult> EditAsync(ISite site, GoogleAuthenticationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAuthentication))
        {
            return null;
        }

        return Initialize<GoogleAuthenticationSettingsViewModel>("GoogleAuthenticationSettings_Edit", model =>
        {
            model.ClientID = settings.ClientID;
            if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication);
                    model.ClientSecret = protector.Unprotect(settings.ClientSecret);
                }
                catch (CryptographicException)
                {
                    _logger.LogError("The client secret could not be decrypted. It may have been encrypted using a different key.");
                    model.ClientSecret = string.Empty;
                    model.HasDecryptionError = true;
                }
            }
            else
            {
                model.ClientSecret = string.Empty;
            }
            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath.Value;
            }
            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, GoogleAuthenticationSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageGoogleAuthentication))
        {
            return null;
        }

        var model = new GoogleAuthenticationSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.ClientID = model.ClientID;
        settings.CallbackPath = model.CallbackPath;
        settings.SaveTokens = model.SaveTokens;

        if (context.Updater.ModelState.IsValid)
        {
            var protector = _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication);

            settings.ClientSecret = protector.Protect(model.ClientSecret);
        }

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
