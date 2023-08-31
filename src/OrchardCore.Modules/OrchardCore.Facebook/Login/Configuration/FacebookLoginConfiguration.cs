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
using OrchardCore.Environment.Shell;
using OrchardCore.Facebook.Login.Services;
using OrchardCore.Facebook.Login.Settings;
using OrchardCore.Facebook.Settings;
using OrchardCore.Modules;

namespace OrchardCore.Facebook.Login.Configuration
{
    [Feature(FacebookConstants.Features.Login)]
    public class FacebookLoginConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<FacebookOptions>
    {
        private readonly FacebookSettings _facebookSettings;
        private readonly IFacebookLoginService _loginService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public FacebookLoginConfiguration(
            IOptions<FacebookSettings> facebookSettings,
            IFacebookLoginService loginService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<FacebookLoginConfiguration> logger)
        {
            _facebookSettings = facebookSettings.Value;
            _loginService = loginService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            if (_facebookSettings == null)
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
            if (!String.Equals(name, FacebookDefaults.AuthenticationScheme))
            {
                return;
            }

            if (_facebookSettings == null)
            {
                return;
            }

            var loginSettings = GetFacebookLoginSettingsAsync().GetAwaiter().GetResult();
            if (loginSettings == null)
            {
                return;
            }

            options.AppId = _facebookSettings.AppId;

            try
            {
                options.AppSecret = _dataProtectionProvider.CreateProtector(FacebookConstants.Features.Core).Unprotect(_facebookSettings.AppSecret);
            }
            catch
            {
                _logger.LogError("The Facebook secret key could not be decrypted. It may have been encrypted using a different key.");
            }

            if (loginSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = loginSettings.CallbackPath;
            }

            options.SaveTokens = loginSettings.SaveTokens;
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
    }
}
