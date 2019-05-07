using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Modules;
using OrchardCore.Github.Services;
using OrchardCore.Github.Settings;

namespace OrchardCore.Github.Configuration
{
    public class GithubOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<GithubOptions>
    {
        private readonly IGithubAuthenticationService _githubAuthenticationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger<GithubOptionsConfiguration> _logger;

        public GithubOptionsConfiguration(
            IGithubAuthenticationService githubAuthenticationService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<GithubOptionsConfiguration> logger)
        {
            _githubAuthenticationService = githubAuthenticationService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = GetGithubAuthenticationSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            if (_githubAuthenticationService.ValidateSettings(settings).Any())
                return;

            // Register the OpenID Connect client handler in the authentication handlers collection.
            options.AddScheme(GithubDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = "Github";
                builder.HandlerType = typeof(GithubHandler);
            });
        }

        public void Configure(string name, GithubOptions options)
        {
            // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, GithubDefaults.AuthenticationScheme, StringComparison.Ordinal))
            {
                return;
            }

            var loginSettings = GetGithubAuthenticationSettingsAsync().GetAwaiter().GetResult();

            options.ClientId = loginSettings?.ClientID ?? string.Empty;

            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(GithubConstants.Features.GithubAuthentication).Unprotect(loginSettings.ClientSecret);
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

        public void Configure(GithubOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<GithubAuthenticationSettings> GetGithubAuthenticationSettingsAsync()
        {
            var settings = await _githubAuthenticationService.GetSettingsAsync();
            if ((_githubAuthenticationService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
            {
                _logger.LogWarning("The Microsoft Account Authentication is not correctly configured.");

                return null;
            }
            return settings;
        }
    }
}
