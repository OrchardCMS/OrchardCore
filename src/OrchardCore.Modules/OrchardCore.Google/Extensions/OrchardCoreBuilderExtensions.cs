using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Google;
using OrchardCore.Google.Authentication.Settings;
using OrchardCore.Secrets;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureGoogleSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            // The 'OrchardCore_Google' section is deprecated and will be removed in a future major version, use 'Authentication:Google' instead.
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>()
                .GetSectionCompat("Authentication:Google", "OrchardCore_Google");

            tenantServices
                .AddOptions<GoogleAuthenticationSettings>()
                .PostConfigure<IDataProtectionProvider, ShellSettings, ILoggerFactory>((settings, dataProtectionProvider, shellSettings, loggerFactory) =>
                {
                    configurationSection.Bind(settings);
                    configurationSection.WarnIfLegacySecretConfigured(
                        loggerFactory.CreateLogger(SecretConfigurationExtensions.LoggerCategory),
                        shellSettings.Name, "Google", "ClientSecret");

                    // Secrets are consumed protected, as when they are saved from the admin settings, so protect the configured ones.
#pragma warning disable CS0618 // Protect legacy credentials supplied through configuration.
                    var clientSecret = configurationSection[nameof(GoogleAuthenticationSettings.ClientSecret)];

                    if (!string.IsNullOrWhiteSpace(clientSecret))
                    {
                        settings.ClientSecret = dataProtectionProvider.CreateProtector(GoogleConstants.Features.GoogleAuthentication).Protect(clientSecret);
                    }
#pragma warning restore CS0618
                });
        });

        return builder;
    }
}
