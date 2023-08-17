using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration
{
    public class GitHubOptionsConfiguration :
        IConfigureOptions<AuthenticationOptions>,
        IConfigureNamedOptions<GitHubOptions>
    {
        private readonly GitHubAuthenticationSettings _gitHubAuthenticationSettings;
        private readonly IDataProtectionProvider _dataProtectionProvider;
        private readonly ILogger _logger;

        public GitHubOptionsConfiguration(
            IOptions<GitHubAuthenticationSettings> gitHubAuthenticationSettings,
            IDataProtectionProvider dataProtectionProvider,
            ILogger<GitHubOptionsConfiguration> logger)
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

            if (_gitHubAuthenticationSettings == null)
            {
                return;
            }

            options.ClientId = _gitHubAuthenticationSettings.ClientID;

            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication).Unprotect(_gitHubAuthenticationSettings.ClientSecret);
            }
            catch
            {
                _logger.LogError("The GitHub Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
            }

            if (_gitHubAuthenticationSettings.CallbackPath.HasValue)
            {
                options.CallbackPath = _gitHubAuthenticationSettings.CallbackPath;
            }

            options.SaveTokens = _gitHubAuthenticationSettings.SaveTokens;
        }

        public void Configure(GitHubOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
    }
}
