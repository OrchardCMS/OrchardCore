using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Secrets;

namespace OrchardCore.Google.Authentication.Configuration;

public class GoogleOptionsConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<GoogleOptions>
{
    private readonly GoogleAuthenticationSettings _googleAuthenticationSettings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ISecretManager _secretManager;
    private readonly ILogger _logger;

    public GoogleOptionsConfiguration(
        IOptions<GoogleAuthenticationSettings> googleAuthenticationSettings,
        IDataProtectionProvider dataProtectionProvider,
        ISecretManager secretManager,
        ILogger<GoogleOptionsConfiguration> logger)
    {
        _googleAuthenticationSettings = googleAuthenticationSettings.Value;
        _dataProtectionProvider = dataProtectionProvider;
        _secretManager = secretManager;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        if (_googleAuthenticationSettings == null)
        {
            return;
        }

#pragma warning disable CS0618 // Type or member is obsolete
        var hasSecret = !string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientSecretSecretName) ||
                        !string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientSecret);
#pragma warning restore CS0618 // Type or member is obsolete

        if (string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientID) || !hasSecret)
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

        // First try to load from secrets
        if (!string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientSecretSecretName))
        {
            try
            {
                var secret = _secretManager.GetSecretAsync<TextSecret>(_googleAuthenticationSettings.ClientSecretSecretName)
                    .GetAwaiter()
                    .GetResult();

                if (secret != null && !string.IsNullOrWhiteSpace(secret.Text))
                {
                    options.ClientSecret = secret.Text;
                    _logger.LogDebug("Google client secret loaded from secret '{SecretName}'.", _googleAuthenticationSettings.ClientSecretSecretName);
                }
                else
                {
                    _logger.LogWarning("Google client secret secret '{SecretName}' was not found or is empty.", _googleAuthenticationSettings.ClientSecretSecretName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load Google client secret from secret '{SecretName}'.", _googleAuthenticationSettings.ClientSecretSecretName);
            }
        }
        // Fall back to legacy encrypted client secret
#pragma warning disable CS0618 // Type or member is obsolete
        else if (!string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientSecret))
        {
            try
            {
                options.ClientSecret = _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication).Unprotect(_googleAuthenticationSettings.ClientSecret);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "The Consumer Secret could not be decrypted. It may have been encrypted using a different key.");
            }
        }
#pragma warning restore CS0618 // Type or member is obsolete

        if (_googleAuthenticationSettings.CallbackPath.HasValue)
        {
            options.CallbackPath = _googleAuthenticationSettings.CallbackPath;
        }

        options.SaveTokens = _googleAuthenticationSettings.SaveTokens;
    }

    public void Configure(GoogleOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
}
