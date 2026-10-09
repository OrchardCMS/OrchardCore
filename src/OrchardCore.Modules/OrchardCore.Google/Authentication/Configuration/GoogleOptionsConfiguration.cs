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
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public GoogleOptionsConfiguration(
        IOptions<GoogleAuthenticationSettings> googleAuthenticationSettings,
        IDataProtectionProvider dataProtectionProvider,
        IServiceProvider serviceProvider,
        ILogger<GoogleOptionsConfiguration> logger)
    {
        _googleAuthenticationSettings = googleAuthenticationSettings.Value;
        _dataProtectionProvider = dataProtectionProvider;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        if (_googleAuthenticationSettings == null)
        {
            return;
        }

        var hasSecret = !string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientSecretSecretName) ||
                        !string.IsNullOrWhiteSpace(_googleAuthenticationSettings.ClientSecret);

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

        options.ClientSecret = _serviceProvider.GetSecretValueAsync(
            _googleAuthenticationSettings.ClientSecretSecretName,
            _googleAuthenticationSettings.ClientSecret,
            _dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication),
            _logger)
            .GetAwaiter()
            .GetResult();

        if (_googleAuthenticationSettings.CallbackPath.HasValue)
        {
            options.CallbackPath = _googleAuthenticationSettings.CallbackPath;
        }

        options.SaveTokens = _googleAuthenticationSettings.SaveTokens;
    }

    public void Configure(GoogleOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
}
