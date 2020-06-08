using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Services;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration
{
    public class MicrosoftAccountOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<MicrosoftAccountOptions>
    {
        private readonly IMicrosoftAccountService _microsoftAccountService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public MicrosoftAccountOptionsConfiguration(
            IMicrosoftAccountService microsoftAccountService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<MicrosoftAccountOptionsConfiguration> logger)
        {
            _microsoftAccountService = microsoftAccountService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = GetMicrosoftAccountSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            if (_microsoftAccountService.ValidateSettings(settings).Any())
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
            if (!string.Equals(name, MicrosoftAccountDefaults.AuthenticationScheme))
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
                _logger.LogError("The Microsoft Account secret key could not be decrypted. It may have been encrypted using a different key.");
            }

            if (loginSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = loginSettings.CallbackPath;
            }
        }

        public void Configure(MicrosoftAccountOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<MicrosoftAccountSettings> GetMicrosoftAccountSettingsAsync()
        {
            var settings = await _microsoftAccountService.GetSettingsAsync();
            if ((_microsoftAccountService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The Microsoft Account Authentication is not correctly configured.");

                return null;
            }
            return settings;
        }
    }
}
