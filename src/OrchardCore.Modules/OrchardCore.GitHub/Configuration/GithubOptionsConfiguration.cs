using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration
{
    public class GitHubOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<GitHubOptions>
    {
        private readonly IGitHubAuthenticationService _githubAuthenticationService;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public GitHubOptionsConfiguration(
            IGitHubAuthenticationService githubAuthenticationService,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<GitHubOptionsConfiguration> logger)
        {
            _githubAuthenticationService = githubAuthenticationService;
            _dataProtectionProvider = dataProtectionProvider;
            _logger = logger;
        }

        public void Configure(AuthenticationOptions options)
        {
            var settings = GetGitHubAuthenticationSettingsAsync().GetAwaiter().GetResult();
            if (settings == null)
            {
                return;
            }

            if (_githubAuthenticationService.ValidateSettings(settings).Any())
            {
                return;
            }

            // Register the OpenID Connect client handler in the authentication handlers collection.
            options.AddScheme(GitHubDefaults.AuthenticationScheme, builder =>
            {
                builder.DisplayName = "GitHub";
                builder.HandlerType = typeof(GitHubHandler);
            });
        }

        public void Configure(string name, GitHubOptions options)
        {
            // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
            if (!string.Equals(name, GitHubDefaults.AuthenticationScheme))
            {
                return;
            }

            var loginSettings = GetGitHubAuthenticationSettingsAsync().GetAwaiter().GetResult();

            options.ClientId = loginSettings?.ClientID ?? string.Empty;

            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication).Unprotect(loginSettings.ClientSecret);
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

        public void Configure(GitHubOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<GitHubAuthenticationSettings> GetGitHubAuthenticationSettingsAsync()
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
