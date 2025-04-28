using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Security.Claims;
using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration;

public class GitHubAuthenticationOptionsConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<GitHubAuthenticationOptions>
{
    private readonly IGitHubAuthenticationService _gitHubAuthenticationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public GitHubAuthenticationOptionsConfiguration(
        IGitHubAuthenticationService gitHubAuthenticationService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<GitHubAuthenticationOptionsConfiguration> logger)
    {
        _gitHubAuthenticationService = gitHubAuthenticationService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        var settings = GetGitHubAuthenticationSettingsAsync()
            .GetAwaiter()
            .GetResult();

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

    public void Configure(string name, GitHubAuthenticationOptions options)
    {
        // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
        if (!string.Equals(name, GitHubAuthenticationDefaults.AuthenticationScheme, StringComparison.Ordinal))
        {
            return;
        }

        var settings = GetGitHubAuthenticationSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (settings == null)
        {
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

    private async Task<GitHubAuthenticationSettings> GetGitHubAuthenticationSettingsAsync()
    {
        var settings = await _gitHubAuthenticationService.GetSettingsAsync();

        if (_gitHubAuthenticationService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            return null;
        }

        return settings;
    }
}
