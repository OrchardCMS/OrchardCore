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
    private readonly GoogleAuthenticationSettings _googleAuthenticationSettings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public GoogleOptionsConfiguration(
        IOptions<GoogleAuthenticationSettings> googleAuthenticationSettings,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<GoogleOptionsConfiguration> logger)
    {
        _googleAuthenticationSettings = googleAuthenticationSettings.Value;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        if (_googleAuthenticationSettings == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientID) ||
            string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientSecret))
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

        if (_googleAuthenticationSettings == null)
        {
            return;
        }

        options.ClientId = _googleAuthenticationSettings.ClientID;
        try
        {
            options.ClientSecret = _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication).Unprotect(_googleAuthenticationSettings.ClientSecret);
        }
        catch
        {
            _logger.LogError("The Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
        }

        if (_googleAuthenticationSettings.CallbackPath.HasValue)
        {
            options.CallbackPath = _googleAuthenticationSettings.CallbackPath;
        }

        options.SaveTokens = _googleAuthenticationSettings.SaveTokens;
    }

    public void Configure(GoogleOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
}
