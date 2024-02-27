using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        var settings = _googleAuthenticationService.GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        if (IsSettingsValid(settings))
        {
            options.CallbackPath = settings.CallbackPath;
            options.ClientID = settings.ClientID;
            options.ClientSecret = settings.ClientSecret;
            options.SaveTokens = settings.SaveTokens;
        }
        else
        {
            if (!IsSettingsValid(options))
            {
                _logger.LogWarning("Google Authentication is not correctly configured.");
            }
        }
    }

    private bool IsSettingsValid(GoogleAuthenticationSettings settings)
    {
        if (_googleAuthenticationService.ValidateSettings(settings).Any(result => result != ValidationResult.Success))
        {
            if (_shellSettings.IsRunning())
            {
                return false;
            }
        }

        return true;
    }
}
