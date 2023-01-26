using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Google.Authentication.Settings;

namespace OrchardCore.Google.Authentication.Configuration
{
    public class GoogleOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<GoogleOptions>
    {
        private readonly GoogleAuthenticationSettings _gitHubAuthenticationSettings;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public GoogleOptionsConfiguration(
            IOptions<GoogleAuthenticationSettings> gitHubAuthenticationSettings,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<GoogleOptionsConfiguration> logger)
        {
            _gitHubAuthenticationSettings = gitHubAuthenticationSettings.Value;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            if (_gitHubAuthenticationSettings == null)
            {
                return;
            }

            options.AddScheme(GoogleDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = "Google";
                builder.HandlerType = typeof(GoogleHandler);
            });
        }

        public void Configure(string name, GoogleOptions options)
        {
            if (!String.Equals(name, GoogleDefaults.AuthenticationScheme))
            {
                return;
            }

            if (_gitHubAuthenticationSettings == null)
            {
                return;
            }

            options.ClientId = _gitHubAuthenticationSettings.ClientID;
            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication).Unprotect(_gitHubAuthenticationSettings.ClientSecret);
            }
            catch
            {
                _logger.LogError("The Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
            }

            if (_gitHubAuthenticationSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = _gitHubAuthenticationSettings.CallbackPath;
            }

            options.SaveTokens = _gitHubAuthenticationSettings.SaveTokens;
        }

        public void Configure(GoogleOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
