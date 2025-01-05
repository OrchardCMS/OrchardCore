using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Options;
using OrchardCore.Google.Authentication.Settings;

namespace OrchardCore.Google.Authentication.Services;

public sealed class GoogleAuthenticationSettingsConfiguration : IConfigureOptions<GoogleAuthenticationSettings>
{
    private readonly GoogleAuthenticationService _googleAuthenticationService;

    public GoogleAuthenticationSettingsConfiguration(GoogleAuthenticationService gitHubAuthenticationService)
    {
        _googleAuthenticationService = gitHubAuthenticationService;
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
            return null;
        }

        return settings;
    }
}
