using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Google.Authentication.Settings;

namespace OrchardCore.Google.Authentication.Services;

public class GoogleAuthenticationSettingsConfiguration : IConfigureOptions<GoogleAuthenticationSettings>
{
    private readonly GoogleAuthenticationService _googleAuthenticationService;
    private readonly ShellSettings _shellSettings;
    private readonly ILogger _logger;

    public GoogleAuthenticationSettingsConfiguration(
        GoogleAuthenticationService gitHubAuthenticationService,
        ShellSettings shellSettings,
        ILogger<GoogleAuthenticationSettingsConfiguration> logger)
    {
        _googleAuthenticationService = gitHubAuthenticationService;
        _shellSettings = shellSettings;
        _logger = logger;
    }

    public void Configure(GoogleAuthenticationSettings options)
    {
        var settings = GetGoogleAuthenticationSettingsAsync()
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

    private async Task<GoogleAuthenticationSettings> GetGoogleAuthenticationSettingsAsync()
    {
        var settings = await _googleAuthenticationService.GetSettingsAsync();

        if ((_googleAuthenticationService.ValidateSettings(settings)).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                _logger.LogWarning("Google Authentication is not correctly configured.");
            }

            return null;
        }

        return settings;
    }
}
