using System.Diagnostics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Microsoft.Authentication.Settings;

namespace OrchardCore.Microsoft.Authentication.Configuration;

public class MicrosoftAccountOptionsConfiguration :
    IConfigureOptions<AuthenticationOptions>,
    IConfigureNamedOptions<MicrosoftAccountOptions>
{
    private readonly IOptionsMonitor<MicrosoftAccountSettings> _microsoftAccountSettings;
    private readonly IDataProtectionProvider _dataProtectionProvider;
    private readonly ILogger _logger;

    public MicrosoftAccountOptionsConfiguration(
        IOptionsMonitor<MicrosoftAccountSettings> microsoftAccountSettings,
        IDataProtectionProvider dataProtectionProvider,
        ILogger<MicrosoftAccountOptionsConfiguration> logger)
    {
        _microsoftAccountSettings = microsoftAccountSettings;
        _dataProtectionProvider = dataProtectionProvider;
        _logger = logger;
    }

    public void Configure(AuthenticationOptions options)
    {
        // Read the current settings, as the options can be rebuilt when the settings are updated.
        var settings = _microsoftAccountSettings.CurrentValue;

        if (settings == null)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.AppId) ||
            string.IsNullOrWhiteSpace(settings.AppSecret))
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

        // Read the current settings, as the options can be rebuilt when the settings are updated.
        var settings = _microsoftAccountSettings.CurrentValue;

        if (settings == null)
        {
            return;
        }

        options.ClientId = settings.AppId;

        try
        {
            options.ClientSecret = _dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount).Unprotect(settings.AppSecret);
        }
        catch
        {
            _logger.LogError("The Microsoft Account secret key could not be decrypted. It may have been encrypted using a different key.");
        }

        if (settings.CallbackPath.HasValue)
        {
            options.CallbackPath = settings.CallbackPath;
        }

        options.SaveTokens = settings.SaveTokens;
    }

    public void Configure(MicrosoftAccountOptions options) => Debug.Fail("This infrastructure method shouldn't be called.");
}
