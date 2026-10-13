using System.Diagnostics;
using System.Security.Claims;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration;

internal sealed class GitHubAuthenticationOptionsConfiguration : IConfigureNamedOptions<GitHubAuthenticationOptions>
{
    private readonly IOptionsMonitor<GitHubAuthenticationSettings> _settings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public GitHubAuthenticationOptionsConfiguration(
        IOptionsMonitor<GitHubAuthenticationSettings> settings,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<GitHubAuthenticationOptionsConfiguration> logger)
    {
        _settings = settings;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(string name, GitHubAuthenticationOptions options)
    {
        // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
        if (!string.Equals(name, GitHubAuthenticationDefaults.AuthenticationScheme, StringComparison.Ordinal))
        {
            return;
        }

        // Read the current settings, as the options can be rebuilt when the settings are updated.
        var settings = _settings.CurrentValue;

        if (settings == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ClientID) ||
            string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            _logger.LogWarning("The GitHub login provider is enabled but not configured.");

            return;
        }

        options.CallbackPath = new PathString("/signin-github");
        options.ClaimActions.MapJsonKey("name", "login");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
        options.ClaimActions.MapJsonKey("url", "url");

        options.ClientId = settings.ClientID;

        try
        {
            options.ClientSecret = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication)
                .Unprotect(settings.ClientSecret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The GitHub Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
        }

        if (settings.CallbackPath.HasValue)
        {
            options.CallbackPath = settings.CallbackPath;
        }

        options.SaveTokens = settings.SaveTokens;
    }

    public void Configure(GitHubAuthenticationOptions options)
        => Debug.Fail("This infrastructure method shouldn't be called.");
}
