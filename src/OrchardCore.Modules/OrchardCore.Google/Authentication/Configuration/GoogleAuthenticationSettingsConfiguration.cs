using System;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Google.Authentication.Services;
using OrchardCore.Google.Authentication.Settings;

namespace OrchardCore.Google.Authentication.Services;

public class GoogleAuthenticationSettingsConfiguration : IConfigureOptions<GoogleAuthenticationSettings>
{
    private readonly GoogleAuthenticationService _googleAuthenticationService;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public GoogleAuthenticationSettingsConfiguration(
        GoogleAuthenticationService gitHubAuthenticationService,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<GoogleAuthenticationSettingsConfiguration> logger)
    {
        _googleAuthenticationService = gitHubAuthenticationService;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(GoogleAuthenticationSettings options)
    {
        var settings = _googleAuthenticationService
            .GetSettingsAsync()
            .GetAwaiter()
            .GetResult();

        options.CallbackPath = settings.CallbackPath;
        options.ClientID = settings.ClientID;
        options.SaveTokens = settings.SaveTokens;

        if (!String.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            try
            {
                var protector = _dataProtectionProvider.CreateProtector(nameof(GoogleAuthenticationSettingsConfiguration));

                options.ClientSecret = protector.Unprotect(settings.ClientSecret);
            }
            catch
            {
                _logger.LogError("The Google app secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }
    }
}
