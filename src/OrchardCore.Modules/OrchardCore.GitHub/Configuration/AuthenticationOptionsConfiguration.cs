using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Settings;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Configuration;

public class AuthenticationOptionsConfiguration : IConfigureOptions<AuthenticationOptions>
{
    private readonly ISiteService _siteService;
    private readonly ILogger _logger;

    public AuthenticationOptionsConfiguration(
        ISiteService siteService,
        ILogger<AuthenticationOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        var settings = _siteService.GetSettings<GitHubAuthenticationSettings>();

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
