using Microsoft.Extensions.Options;
using OrchardCore.Google.Authentication.Settings;

namespace OrchardCore.Google.Authentication.Services;

public class GoogleAuthenticationSettingsConfiguration : IConfigureOptions<GoogleAuthenticationSettings>
{
    private readonly GoogleAuthenticationService _googleAuthenticationService;

    public GoogleAuthenticationSettingsConfiguration(GoogleAuthenticationService gitHubAuthenticationService)
    {
        _googleAuthenticationService = gitHubAuthenticationService;
    }

    public void Configure(GoogleAuthenticationSettings options)
    {
        var settings = _googleAuthenticationService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.CallbackPath = settings.CallbackPath;
        options.ClientID = settings.ClientID;
        options.ClientSecret = settings.ClientSecret;
        options.SaveTokens = settings.SaveTokens;
    }
}
