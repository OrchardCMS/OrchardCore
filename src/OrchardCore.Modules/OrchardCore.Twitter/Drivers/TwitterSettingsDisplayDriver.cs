using System.Security.Cryptography;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Entities;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;
using OrchardCore.Environment.Shell;
using OrchardCore.Settings;
using OrchardCore.Twitter.Settings;
using OrchardCore.Twitter.ViewModels;

namespace OrchardCore.Twitter.Drivers;

public sealed class TwitterSettingsDisplayDriver : SiteDisplayDriver<TwitterSettings>
{
    private readonly IShellReleaseManager _shellReleaseManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger _logger;

    public TwitterSettingsDisplayDriver(
        IShellReleaseManager shellReleaseManager,
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        ILogger<TwitterSettingsDisplayDriver> logger)
    {
        _shellReleaseManager = shellReleaseManager;
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override string SettingsGroupId
        => TwitterConstants.Features.Twitter;

    public override async Task<IDisplayResult> EditAsync(ISite site, TwitterSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterSignin))
        {
            return null;
        }

        return Initialize<TwitterSettingsViewModel>("TwitterSettings_Edit", model =>
        {
            model.APIKey = settings.ConsumerKey;
            if (!string.IsNullOrWhiteSpace(settings.ConsumerSecret))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
                    model.APISecretKey = protector.Unprotect(settings.ConsumerSecret);
                }
                catch (CryptographicException)
                {
                    _logger.LogError("The API secret key could not be decrypted. It may have been encrypted using a different key.");
                    model.APISecretKey = string.Empty;
                    model.HasDecryptionError = true;
                }
            }
            else
            {
                model.APISecretKey = string.Empty;
            }
            model.AccessToken = settings.AccessToken;
            if (!string.IsNullOrWhiteSpace(settings.AccessTokenSecret))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
                    model.AccessTokenSecret = protector.Unprotect(settings.AccessTokenSecret);
                }
                catch (CryptographicException)
                {
                    _logger.LogError("The access token secret could not be decrypted. It may have been encrypted using a different key.");
                    model.AccessTokenSecret = string.Empty;
                    model.HasDecryptionError = true;
                }
            }
            else
            {
                model.AccessTokenSecret = string.Empty;
            }
        }).Location("Content:5")
        .OnGroup(SettingsGroupId);
    }

    public override async Task<IDisplayResult> UpdateAsync(ISite site, TwitterSettings settings, UpdateEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitter))
        {
            return null;
        }

        var model = new TwitterSettingsViewModel();
        await context.Updater.TryUpdateModelAsync(model, Prefix);

        settings.ConsumerKey = model.APIKey;
        settings.AccessToken = model.AccessToken;

        if (context.Updater.ModelState.IsValid)
        {
            var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);

            settings.ConsumerSecret = protector.Protect(model.APISecretKey);
            settings.AccessTokenSecret = protector.Protect(model.AccessTokenSecret);
        }

        _shellReleaseManager.RequestRelease();

        return await EditAsync(site, settings, context);
    }
}
