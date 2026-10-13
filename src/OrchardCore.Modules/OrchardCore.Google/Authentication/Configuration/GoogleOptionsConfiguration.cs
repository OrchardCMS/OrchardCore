using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Google.Authentication.Settings;

namespace OrchardCore.Google.Authentication.Configuration;

public class GoogleOptionsConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<GoogleOptions>
{
    private readonly IOptionsMonitor<GoogleAuthenticationSettings> _googleAuthenticationSettings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public GoogleOptionsConfiguration(
        IOptionsMonitor<GoogleAuthenticationSettings> googleAuthenticationSettings,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<GoogleOptionsConfiguration> logger)
    {
        _googleAuthenticationSettings = googleAuthenticationSettings;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        // Read the current settings, as the options can be rebuilt when the settings are updated.
        var settings = _googleAuthenticationSettings.CurrentValue;

        if (settings == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.ClientID) ||
            string.IsNullOrWhiteSpace(settings.ClientSecret))
        {
            _logger.LogWarning("The Google login provider is enabled but not configured.");

            return;
        }

        options.AddScheme(GoogleDefaults.AuthenticationScheme, builder =>
        {
            builder.DisplayName = "Google";
            builder.HandlerType = typeof(GoogleHandler);
        });
    }

    public void Configure(string name, GoogleOptions options)
    {
        if (!string.Equals(name, GoogleDefaults.AuthenticationScheme, StringComparison.Ordinal))
        {
            return;
        }

        var settings = _googleAuthenticationSettings.CurrentValue;

        if (settings == null)
        {
            return;
        }

        options.ClientId = settings.ClientID;
        try
        {
            options.ClientSecret = _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication).Unprotect(settings.ClientSecret);
        }
        catch
        {
            _logger.LogError("The Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
        }

        if (settings.CallbackPath.HasValue)
        {
            options.CallbackPath = settings.CallbackPath;
        }

        options.SaveTokens = settings.SaveTokens;
    }

    public void Configure(GoogleOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
}
