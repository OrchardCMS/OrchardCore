using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    [Feature(MicrosoftAuthenticationConstants.Features.AAD)]
    public class AzureADAuthenticationConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<MicrosoftAccountOptions>
    {
        private readonly IAzureADAuthenticationService _coreService;
        private readonly IMicrosoftAuthenticationService _loginService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<FacebookLoginConfiguration> _logger;

        public FacebookLoginConfiguration(
            IAzureADAuthenticationService coreService,
            IMicrosoftAuthenticationService loginService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<FacebookLoginConfiguration> logger)
        {
            _coreService = coreService;
            _loginService = loginService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            var coreSettings = GetFacebookCoreSettingsAsync().GetAwaiter().GetResult();
            if (coreSettings == null)
            {
                return;
            }

            var loginSettings = GetFacebookLoginSettingsAsync().GetAwaiter().GetResult();
            if (loginSettings == null)
            {
                return;
            }

            // Register the OpenID Connect client handler in the authentication handlers collection.
            options.AddScheme(FacebookDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = "Facebook";
                builder.HandlerType = typeof(FacebookHandler);
            });
        }

        public void Configure(string name, FacebookOptions options)
        {
            // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, FacebookDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            var coreSettings = GetFacebookCoreSettingsAsync().GetAwaiter().GetResult();
            if (coreSettings == null)
            {
                return;
            }

            var loginSettings = GetFacebookLoginSettingsAsync().GetAwaiter().GetResult();
            if (loginSettings == null)
            {
                return;
            }
            options.AppId = coreSettings.AppId;

            try
            {
                options.AppSecret = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core).Unprotect(coreSettings.AppSecret);
            }
            catch
            {
                _logger.LogError("The Facebook secret keycould not be decrypted. It may have been encrypted using a different key.");
            }

            if (loginSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = loginSettings.CallbackPath;
            }
        }

        public void Configure(FacebookOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<MicrosoftAuthenticationSettings> GetFacebookLoginSettingsAsync()
        {
            var settings = await _loginService.GetSettingsAsync();
            if ((await _loginService.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The Facebook Login module is not correctly configured.");

                return null;
            }
            return settings;
        }

        private async Task<AzureADAuthenticationSettings> GetFacebookCoreSettingsAsync()
        {
            var settings = await _coreService.GetSettingsAsync();
            if ((await _coreService.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The Facebook Core module is not correctly configured.");

                return null;
            }

            return settings;
        }

    }
}
