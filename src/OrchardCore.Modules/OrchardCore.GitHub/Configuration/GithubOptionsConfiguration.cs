using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Models;
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
        private readonly ShellSettings _shellSettings;
        private readonly ILogger _logger;

        public GitHubOptionsConfiguration(
            IGitHubAuthenticationService githubAuthenticationService,
            IDataProtectionProvider dataProtectionProvider,
            ShellSettings shellSettings,
            ILogger<GitHubOptionsConfiguration> logger)
        {
            _githubAuthenticationService = githubAuthenticationService;
            _dataProtectionProvider = dataProtectionProvider;
            _shellSettings = shellSettings;
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
            if (!String.Equals(name, GitHubDefaults.AuthenticationScheme))
            {
                return;
            }

            var loginSettings = GetGitHubAuthenticationSettingsAsync().GetAwaiter().GetResult();
            if (loginSettings == null)
            {
                return;
            }

            options.ClientId = loginSettings.ClientID;

            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication).Unprotect(loginSettings.ClientSecret);
            }
            catch
            {
                _logger.LogError("The GitHub Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
            }

            if (loginSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = loginSettings.CallbackPath;
            }

            options.SaveTokens = loginSettings.SaveTokens;
        }

        public void Configure(GitHubOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");

        private async Task<GitHubAuthenticationSettings> GetGitHubAuthenticationSettingsAsync()
        {
            var settings = await _githubAuthenticationService.GetSettingsAsync();
            if ((_githubAuthenticationService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
            {
                if (_shellSettings.State == TenantState.Running)
                {
                    _logger.LogWarning("GitHub Authentication is not correctly configured.");
                }

                return null;
            }

            return settings;
        }
    }
}
