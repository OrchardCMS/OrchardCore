using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration;

public sealed class GitHubAuthenticationSettingsConfiguration : IConfigureOptions<GitHubAuthenticationSettings>
{
    private readonly IGitHubAuthenticationService _gitHubAuthenticationService;

    public GitHubAuthenticationSettingsConfiguration(IGitHubAuthenticationService gitHubAuthenticationService)
    {
        _gitHubAuthenticationService = gitHubAuthenticationService;
    }

    public void Configure(GitHubAuthenticationSettings options)
    {
        var settings = GetGitHubAuthenticationSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (settings != null)
        {
            options.CallbackPath = settings.CallbackPath;
            options.ClientID = settings.ClientID;
            options.ClientSecret = settings.ClientSecret;
            options.SaveTokens = settings.SaveTokens;
        }
    }

    private async Task<GitHubAuthenticationSettings> GetGitHubAuthenticationSettingsAsync()
    {
        var settings = await _gitHubAuthenticationService.GetSettingsAsync();

        if ((_gitHubAuthenticationService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
        {
            return null;
        }

        return settings;
    }
}
