using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
        private readonly FacebookLoginSettings _facebookLoginSettings;
        private readonly IFacebookLoginService _loginService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public FacebookLoginConfiguration(
            IOptions<FacebookSettings> facebookSettings,
            IOptions<FacebookLoginSettings> facebookLoginSettings,
            IFacebookLoginService loginService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<FacebookLoginConfiguration> logger)
        {
            _facebookSettings = facebookSettings.Value;
            _facebookLoginSettings = facebookLoginSettings.Value;
            _loginService = loginService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            if (_facebookSettings == null || _facebookLoginSettings == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(_facebookSettings.AppId) || string.IsNullOrWhiteSpace(_facebookSettings.AppSecret))
            {
                _logger.LogWarning("The Facebook login provider is enabled but not configured.");

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
            if (!string.Equals(name, FacebookDefaults.AuthenticationScheme))
            {
                return;
            }

            if (_facebookSettings == null || _facebookLoginSettings == null)
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

            if (_facebookLoginSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = _facebookLoginSettings.CallbackPath;
            }

            options.SaveTokens = _facebookLoginSettings.SaveTokens;
        }

        public void Configure(FacebookOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
