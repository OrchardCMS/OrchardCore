using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration;

internal sealed class AuthenticationOptionsConfiguration : IConfigureOptions<AuthenticationOptions>
{
    private readonly IOptionsMonitor<GitHubAuthenticationSettings> _settings;
    private readonly ILogger _logger;

    public AuthenticationOptionsConfiguration(
        IOptionsMonitor<GitHubAuthenticationSettings> settings,
        ILogger<AuthenticationOptionsConfiguration> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
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

        // Register the OpenID Connect client handler in the authentication handlers collection.
        options.AddScheme<GitHubAuthenticationHandler>(GitHubAuthenticationDefaults.AuthenticationScheme, GitHubAuthenticationDefaults.DisplayName);
    }
}
