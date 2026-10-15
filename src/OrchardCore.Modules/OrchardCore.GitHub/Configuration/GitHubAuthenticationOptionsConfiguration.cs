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
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public GitHubAuthenticationOptionsConfiguration(
        ISiteService siteService,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        ILogger<GitHubAuthenticationOptionsConfiguration> logger)
    {
        _siteService = siteService;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
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

        var hasSecret = !string.IsNullOrWhiteSpace(settings.ClientSecretSecretName) ||
                        !string.IsNullOrWhiteSpace(settings.ClientSecret);

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

        options.ClientSecret = _serviceProvider.GetSecretValueAsync(
            settings.ClientSecretSecretName,
            settings.ClientSecret,
            _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication),
            _logger)
            .GetAwaiter()
            .GetResult();

        if (settings.CallbackPath.HasValue)
        {
            options.CallbackPath = settings.CallbackPath;
        }

        options.SaveTokens = settings.SaveTokens;
    }

    public void Configure(GitHubAuthenticationOptions options)
        => Debug.Fail("This infrastructure method shouldn't be called.");
}
