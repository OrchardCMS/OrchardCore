using Microsoft.Extensions.Options;
using OrchardCore.GitHub.Settings;
using OrchardCore.Settings;

namespace OrchardCore.GitHub.Configuration;

/// <summary>
/// Configures the <see cref="GitHubAuthenticationSettings"/> options from the site settings, so that they
/// can be overridden by the tenant configuration.
/// </summary>
internal sealed class GitHubAuthenticationSettingsConfiguration : IConfigureOptions<GitHubAuthenticationSettings>
{
    private readonly ISiteService _siteService;

    public GitHubAuthenticationSettingsConfiguration(ISiteService siteService)
    {
        _siteService = siteService;
    }

    public void Configure(GitHubAuthenticationSettings options)
    {
        var settings = _siteService.GetSettings<GitHubAuthenticationSettings>();

        options.ClientID = settings.ClientID;
        options.ClientSecret = settings.ClientSecret;
        options.CallbackPath = settings.CallbackPath;
        options.SaveTokens = settings.SaveTokens;
    }
}
