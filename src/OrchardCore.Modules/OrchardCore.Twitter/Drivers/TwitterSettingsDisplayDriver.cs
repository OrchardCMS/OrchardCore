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

namespace OrchardCore.Twitter.Drivers
{
    public class TwitterSettingsDisplayDriver : SectionDisplayDriver<ISite, TwitterSettings>
    {
        private readonly IAuthorizationService _authorizationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IShellHost _shellHost;
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public TwitterSettingsDisplayDriver(
            IAuthorizationService authorizationService,
            IDataProtectionProvider dataProtectionProvider,
            IHttpContextAccessor httpContextAccessor,
            IShellHost shellHost,
            ShellSettings shellSettings,
            ILogger<TwitterSettingsDisplayDriver> logger)
        {
            _authorizationService = authorizationService;
            _dataProtectionProvider = dataProtectionProvider;
            _httpContextAccessor = httpContextAccessor;
            _shellHost = shellHost;
            _shellSettings = shellSettings;
            _logger = logger;
        }

        public override async Task<IDisplayResult> EditAsync(TwitterSettings settings, BuildEditorContext context)
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
            }).Location("Content:5").OnGroup(TwitterConstants.Features.Twitter);
        }

        public override async Task<IDisplayResult> UpdateAsync(TwitterSettings settings, BuildEditorContext context)
        {
            if (context.GroupId == TwitterConstants.Features.Twitter)
            {
                var user = _httpContextAccessor.HttpContext?.User;
                if (!await _authorizationService.AuthorizeAsync(user, Permissions.ManageTwitter))
                {
                    return null;
                }

                var model = new TwitterSettingsViewModel();
                await context.Updater.TryUpdateModelAsync(model, Prefix);

                if (context.Updater.ModelState.IsValid)
                {
                    var protector = _dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);

                    settings.ConsumerKey = model.APIKey;
                    settings.ConsumerSecret = protector.Protect(model.APISecretKey);
                    settings.AccessToken = model.AccessToken;
                    settings.AccessTokenSecret = protector.Protect(model.AccessTokenSecret);
                    await _shellHost.ReleaseShellContextAsync(_shellSettings);
                }
            }
            return await EditAsync(settings, context);
        }
    }
}
