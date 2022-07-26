using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
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

public class TwitterAuthenticationSettingsDisplayDriver : SectionDisplayDriver<ISite, TwitterAuthenticationSettings>
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IShellHost _shellHost;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public TwitterAuthenticationSettingsDisplayDriver(
        IAuthorizationService authorizationService,
        IDataProtectionProvider dataProtectionProvider,
        IHttpContextAccessor httpContextAccessor,
        IShellHost shellHost,
        ShellSettings shellSettings,
        ILogger<TwitterAuthenticationSettingsDisplayDriver> logger)
    {
        _authorizationService = authorizationService;
        _dataProtectionProvider = dataProtectionProvider;
        _httpContextAccessor = httpContextAccessor;
        _shellHost = shellHost;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public override async Task<IDisplayResult> EditAsync(TwitterAuthenticationSettings settings, BuildEditorContext context)
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterAuthentication))
        {
            return null;
        }

        return Initialize<TwitterAuthenticationSettingsViewModel>("TwitterAuthenticationSettings_Edit", model =>
        {
            model.ConsumerKey = settings.ConsumerKey;

            if (!String.IsNullOrWhiteSpace(settings.ConsumerSecret))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterAuthentication);
                    model.ConsumerSecret = protector.Unprotect(settings.ConsumerSecret);
                }
                catch (CryptographicException)
                {
                    _logger.LogError("The API secret key could not be decrypted. It may have been encrypted using a different key.");
                    model.ConsumerSecret = String.Empty;
                    model.HasDecryptionError = true;
                }
            }
            else
            {
                model.ConsumerSecret = String.Empty;
            }

            model.AccessToken = settings.AccessToken;

            if (!String.IsNullOrWhiteSpace(settings.AccessTokenSecret))
            {
                try
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterAuthentication);
                    model.AccessTokenSecret = protector.Unprotect(settings.AccessTokenSecret);
                }
                catch (CryptographicException)
                {
                    _logger.LogError("The access token secret could not be decrypted. It may have been encrypted using a different key.");
                    model.AccessTokenSecret = String.Empty;
                    model.HasDecryptionError = true;
                }
            }
            else
            {
                model.AccessTokenSecret = String.Empty;
            }

            if (settings.CallbackPath.HasValue)
            {
                model.CallbackPath = settings.CallbackPath;
            }

            model.SaveTokens = settings.SaveTokens;
        }).Location("Content:5").OnGroup(TwitterConstants.Features.TwitterAuthentication);
    }

    public override async Task<IDisplayResult> UpdateAsync(TwitterAuthenticationSettings settings, BuildEditorContext context)
    {
        if (context.GroupId == TwitterConstants.Features.TwitterAuthentication)
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitterAuthentication))
            {
                return null;
            }

            var model = new TwitterAuthenticationSettingsViewModel();
            await context.Updater.TryUpdateModelAsync(model, Prefix);

            if (context.Updater.ModelState.IsValid)
            {
                var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.TwitterAuthentication);

                settings.ConsumerKey = model.ConsumerKey;
                settings.ConsumerSecret = protector.Protect(model.ConsumerSecret);
                settings.AccessToken = model.AccessToken;
                settings.AccessTokenSecret = protector.Protect(model.AccessTokenSecret);
                settings.CallbackPath = model.CallbackPath;
                settings.SaveTokens = model.SaveTokens;

                await _shellHost.ReleaseShellContextAsync(_shellSettings);
            }
        }
        return await EditAsync(settings, context);
    }
}
