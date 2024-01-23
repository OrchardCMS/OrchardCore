using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.GitHub.Services;
using OrchardCore.GitHub.Settings;

namespace OrchardCore.GitHub.Configuration;

public class GitHubAuthenticationSettingsConfiguration : IConfigureOptions<GitHubAuthenticationSettings>
{
    private readonly IGitHubAuthenticationService _gitHubAuthenticationService;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public GitHubAuthenticationSettingsConfiguration(
        IGitHubAuthenticationService gitHubAuthenticationService,
        ShellSettings shellSettings,
        ILogger<GitHubAuthenticationSettingsConfiguration> logger)
    {
        _gitHubAuthenticationService = gitHubAuthenticationService;
        _shellSettings = shellSettings;
        _logger = logger;
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
            if (_shellSettings.IsRunning())
            {
                _logger.LogWarning("GitHub Authentication is not correctly configured.");
            }

            return null;
        }

        return settings;
    }
}
