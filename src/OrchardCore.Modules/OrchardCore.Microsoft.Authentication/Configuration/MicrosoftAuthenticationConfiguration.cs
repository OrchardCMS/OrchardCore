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
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    [Feature(MicrosoftAuthenticationConstants.Features.MicrosoftAccount)]
    public class MicrosoftAuthenticationConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<MicrosoftAccountOptions>
    {
        private readonly IMicrosoftAuthenticationService _loginService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<MicrosoftAuthenticationConfiguration> _logger;

        public MicrosoftAuthenticationConfiguration(
            IMicrosoftAuthenticationService loginService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<MicrosoftAuthenticationConfiguration> logger)
        {
            _loginService = loginService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            var loginSettings = GetMicrosoftAccountSettingsAsync().GetAwaiter().GetResult();
            if (loginSettings == null)
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
            if (!string.Equals(name, MicrosoftAccountDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            var loginSettings = GetMicrosoftAccountSettingsAsync().GetAwaiter().GetResult();

            options.ClientId = loginSettings?.AppId ?? string.Empty;

            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount).Unprotect(loginSettings.AppSecret);
            }
            catch
            {
                _logger.LogError("The MicrosoftAccount secret keycould not be decrypted. It may have been encrypted using a different key.");
            }

            if (loginSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = loginSettings.CallbackPath;
            }
        }

        public void Configure(MicrosoftAccountOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<MicrosoftAuthenticationSettings> GetMicrosoftAccountSettingsAsync()
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
