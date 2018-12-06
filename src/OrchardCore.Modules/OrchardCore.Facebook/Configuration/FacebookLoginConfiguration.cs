using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using OrchardCore.Modules;
using OrchardCore.Facebook.Services;
using OrchardCore.Facebook.Settings;

namespace OrchardCore.Facebook.Configuration
{
    [Feature(FacebookConstants.Features.Login)]
    public class FacebookLoginConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<FacebookOptions>
    {
        private readonly IFacebookCoreService _coreService;
        private readonly IFacebookLoginService _loginService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<FacebookLoginConfiguration> _logger;

        public FacebookLoginConfiguration(
            IFacebookCoreService coreService,
            IFacebookLoginService loginService,
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

        private async Task<FacebookLoginSettings> GetFacebookLoginSettingsAsync()
        {
            var settings = await _loginService.GetSettingsAsync();
            if ((await _loginService.ValidateSettingsAsync(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The Facebook Login module is not correctly configured.");

                return null;
            }
            return settings;
        }

        private async Task<FacebookCoreSettings> GetFacebookCoreSettingsAsync()
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
