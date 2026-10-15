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
    private readonly IServiceProvider _serviceProvider;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public MicrosoftAccountOptionsConfiguration(
        IOptions<MicrosoftAccountSettings> microsoftAccountSettings,
        IServiceProvider serviceProvider,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<MicrosoftAccountOptionsConfiguration> logger)
    {
        _microsoftAccountSettings = microsoftAccountSettings.Value;
        _serviceProvider = serviceProvider;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        if (_microsoftAccountSettings == null)
        {
            return;
        }

        var hasAppSecret = !string.IsNullOrWhiteSpace(_microsoftAccountSettings.AppSecretSecretName) ||
                           !string.IsNullOrWhiteSpace(_microsoftAccountSettings.AppSecret);

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

        options.ClientSecret = _serviceProvider.GetSecretValueAsync(
            _microsoftAccountSettings.AppSecretSecretName,
            _microsoftAccountSettings.AppSecret,
            _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount),
            _logger)
            .GetAwaiter()
            .GetResult();

        if (_microsoftAccountSettings.CallbackPath.HasValue)
        {
            options.CallbackPath = _microsoftAccountSettings.CallbackPath;
        }

        options.SaveTokens = _microsoftAccountSettings.SaveTokens;
    }

    public void Configure(MicrosoftAccountOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
}
