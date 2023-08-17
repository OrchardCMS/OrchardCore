using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    public class MicrosoftAccountOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<MicrosoftAccountOptions>
    {
        private readonly MicrosoftAccountSettings _microsoftAccountSettings;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public MicrosoftAccountOptionsConfiguration(
            IOptions<MicrosoftAccountSettings> microsoftAccountSettings,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<MicrosoftAccountOptionsConfiguration> logger)
        {
            _microsoftAccountSettings = microsoftAccountSettings.Value;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            if (_microsoftAccountSettings == null)
            {
                return;
            }

            // Register the OpenID Connect client handler in the authentication handlers collection.
            options.AddScheme(MicrosoftAccountDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = "Microsoft Account";
                builder.HandlerType = typeof(MicrosoftAccountHandler);
            });
        }

        public void Configure(string name, MicrosoftAccountOptions options)
        {
            // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
            if (!String.Equals(name, MicrosoftAccountDefaults.AuthenticationScheme))
            {
                return;
            }

            if (_microsoftAccountSettings == null)
            {
                return;
            }

            options.ClientId = _microsoftAccountSettings.AppId;

            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount).Unprotect(_microsoftAccountSettings.AppSecret);
            }
            catch
            {
                _logger.LogError("The Microsoft Account secret key could not be decrypted. It may have been encrypted using a different key.");
            }

            if (_microsoftAccountSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = _microsoftAccountSettings.CallbackPath;
            }

            options.SaveTokens = _microsoftAccountSettings.SaveTokens;
        }

        public void Configure(MicrosoftAccountOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
