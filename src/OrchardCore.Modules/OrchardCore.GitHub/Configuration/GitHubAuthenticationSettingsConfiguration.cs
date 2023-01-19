using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration;

public class GitHubAuthenticationSettingsConfiguration : IConfigureOptions<GitHubAuthenticationSettings>
{
    private readonly IGitHubAuthenticationService _gitHubAuthenticationService;

    public GitHubAuthenticationSettingsConfiguration(IGitHubAuthenticationService gitHubAuthenticationService)
    {
        _gitHubAuthenticationService = gitHubAuthenticationService;
    }

    public void Configure(GitHubAuthenticationSettings options)
    {
        var settings = _gitHubAuthenticationService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.CallbackPath = settings.CallbackPath;
        options.ClientID = settings.ClientID;
        options.ClientSecret = settings.ClientSecret;
        options.SaveTokens = settings.SaveTokens;
    }
}
