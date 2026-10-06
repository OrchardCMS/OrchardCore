using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Secrets;

namespace OrchardCore.Microsoft.Authentication.Configuration;

public class MicrosoftAccountOptionsConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<MicrosoftAccountOptions>
{
    private readonly MicrosoftAccountSettings _microsoftAccountSettings;
    private readonly ISecretManager _secretManager;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public MicrosoftAccountOptionsConfiguration(
        IOptions<MicrosoftAccountSettings> microsoftAccountSettings,
        ISecretManager secretManager,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<MicrosoftAccountOptionsConfiguration> logger)
    {
        _microsoftAccountSettings = microsoftAccountSettings.Value;
        _secretManager = secretManager;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        if (_microsoftAccountSettings == null)
        {
            return;
        }

        // Check if configured with either new secrets or legacy settings.
#pragma warning disable CS0618 // Type or member is obsolete
        var hasAppSecret = !string.IsNullOrWhiteSpace(_microsoftAccountSettings.AppSecretSecretName) ||
                           !string.IsNullOrWhiteSpace(_microsoftAccountSettings.AppSecret);
#pragma warning restore CS0618 // Type or member is obsolete

        if (string.IsNullOrWhiteSpace(_microsoftAccountSettings.AppId) || !hasAppSecret)
        {
            _logger.LogWarning("The Microsoft login provider is enabled but not configured.");

            return;
        }

        // Register the OpenID Connect client handler in the authentication handlers collection.
        options.AddScheme(MicrosoftAccountDefaults.AuthenticationScheme, builder =>
        {
            builder.DisplayName = "Microsoft Account";
            builder.HandlerType = typeof(MicrosoftAccountHandler);
        });
    }

    public void Configure(string name, MicrosoftAccountOptions options)
    {
        // Ignore OpenID Connect client handler instances that don't correspond to the instance managed by the OpenID module.
        if (!string.Equals(name, MicrosoftAccountDefaults.AuthenticationScheme, StringComparison.Ordinal))
        {
            return;
        }

        if (_microsoftAccountSettings == null)
        {
            return;
        }

        options.ClientId = _microsoftAccountSettings.AppId;

        // Try to get the secret from the Secrets module first.
        if (!string.IsNullOrWhiteSpace(_microsoftAccountSettings.AppSecretSecretName))
        {
            var secret = _secretManager.GetSecretAsync<TextSecret>(_microsoftAccountSettings.AppSecretSecretName)
                .GetAwaiter()
                .GetResult();

            if (secret != null)
            {
                options.ClientSecret = secret.Text;
            }
            else
            {
                _logger.LogError("The Microsoft Account secret '{SecretName}' could not be found.", _microsoftAccountSettings.AppSecretSecretName);
            }
        }
        else
        {
            // Fall back to legacy encrypted setting.
#pragma warning disable CS0618 // Type or member is obsolete
            if (!string.IsNullOrWhiteSpace(_microsoftAccountSettings.AppSecret))
            {
                try
                {
                    options.ClientSecret = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount).Unprotect(_microsoftAccountSettings.AppSecret);
                }
                catch
                {
                    _logger.LogError("The Microsoft Account secret key could not be decrypted. It may have been encrypted using a different key.");
                }
            }
#pragma warning restore CS0618 // Type or member is obsolete
        }

        if (_microsoftAccountSettings.CallbackPath.HasValue)
        {
            options.CallbackPath = _microsoftAccountSettings.CallbackPath;
        }

        options.SaveTokens = _microsoftAccountSettings.SaveTokens;
    }

    public void Configure(MicrosoftAccountOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
}
