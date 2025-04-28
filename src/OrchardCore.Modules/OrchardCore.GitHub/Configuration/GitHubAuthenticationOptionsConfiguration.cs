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

public class GitHubAuthenticationOptionsConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<GitHubAuthenticationOptions>
{
    private readonly GitHubAuthenticationSettings _gitHubAuthenticationSettings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public GitHubAuthenticationOptionsConfiguration(
        IOptions<GitHubAuthenticationSettings> gitHubAuthenticationSettings,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<GitHubAuthenticationOptionsConfiguration> logger)
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

        if (string.IsNullOrWhiteSpace(_gitHubAuthenticationSettings.ClientID) ||
            string.IsNullOrWhiteSpace(_gitHubAuthenticationSettings.ClientSecret))
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

        if (_gitHubAuthenticationSettings == null)
        {
            return;
        }

        options.CallbackPath = new PathString("/signin-github");
        options.ClaimActions.MapJsonKey("name", "login");
        options.ClaimActions.MapJsonKey(ClaimTypes.Email, "email", ClaimValueTypes.Email);
        options.ClaimActions.MapJsonKey("url", "url");

        options.ClientId = _gitHubAuthenticationSettings.ClientID;

        try
        {
            options.ClientSecret = _dataProtectionProvider.CreateProtector(GitHubConstants.Features.GitHubAuthentication)
                .Unprotect(_gitHubAuthenticationSettings.ClientSecret);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "The GitHub Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
        }

        if (_gitHubAuthenticationSettings.CallbackPath.HasValue)
        {
            options.CallbackPath = _gitHubAuthenticationSettings.CallbackPath;
        }

        options.SaveTokens = _gitHubAuthenticationSettings.SaveTokens;
    }

    public void Configure(GitHubAuthenticationOptions options)
        => Debug.Fail("This infrastructure method shouldn't be called.");
}
