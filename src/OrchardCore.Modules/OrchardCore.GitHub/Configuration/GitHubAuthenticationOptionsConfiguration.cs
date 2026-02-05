using System.Diagnostics;
using System.Security.Claims;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Settings;
using OrchardCore.Secrets;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Configuration;

internal sealed class GitHubAuthenticationOptionsConfiguration : IConfigureNamedOptions<GitHubAuthenticationOptions>
{
    private readonly ISiteService _siteService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ISecretManager _secretManager;
    private readonly ILogger _logger;

    public GitHubAuthenticationOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        ISecretManager secretManager,
        ILogger<GitHubAuthenticationOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _secretManager = secretManager;
        _logger = logger;
    }

    public void Configure(string name, GitHubAuthenticationOptions options)
    {
        // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
        if (!string.Equals(name, GitHubAuthenticationDefaults.AuthenticationScheme, StringComparison.Ordinal))
        {
            return;
        }

        var settings = _siteService.GetSettings<GitHubAuthenticationSettings>();

        if (settings == null)
        {
            return;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        var hasSecret = !string.IsNullOrWhiteSpace(settings.ClientSecretSecretName) ||
                        !string.IsNullOrWhiteSpace(settings.ClientSecret);
#pragma warning restore CS0618 // Type or member is obsolete

        if (string.IsNullOrWhiteSpace(settings.ClientID) || !hasSecret)
        {
            _logger.LogWarning("The GitHub login provider is enabled but not configured.");

            return;
        }

        options.CallbackPath = new PathString("/signin-github");
        options.ClaimActions.MapJsonKey("name", "login");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
        options.ClaimActions.MapJsonKey("url", "url");

        options.ClientId = settings.ClientID;

        // First try to load from secrets
        if (!string.IsNullOrWhiteSpace(settings.ClientSecretSecretName))
        {
            try
            {
                var secret = _secretManager.GetSecretAsync<TextSecret>(settings.ClientSecretSecretName)
                    .GetAwaiter()
                    .GetResult();

                if (secret != null && !string.IsNullOrWhiteSpace(secret.Text))
                {
                    options.ClientSecret = secret.Text;
                    _logger.LogDebug("GitHub client secret loaded from secret '{SecretName}'.", settings.ClientSecretSecretName);
                }
                else
                {
                    _logger.LogWarning("GitHub client secret secret '{SecretName}' was not found or is empty.", settings.ClientSecretSecretName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load GitHub client secret from secret '{SecretName}'.", settings.ClientSecretSecretName);
            }
        }
        // Fall back to legacy encrypted client secret
#pragma warning disable CS0618 // Type or member is obsolete
        else if (!string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication)
                    .Unprotect(settings.ClientSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The GitHub Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (settings.CallbackPath.HasValue)
        {
            options.CallbackPath = settings.CallbackPath;
        }

        options.SaveTokens = settings.SaveTokens;
    }

    public void Configure(GitHubAuthenticationOptions options)
        => Debug.Fail("This infrastructure method shouldn't be called.");
}
