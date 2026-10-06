using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Microsoft.Authentication;
using OrchardCore.Microsoft.Authentication.Settings;
using OrchardCore.Secrets;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureMicrosoftAccountSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            // The 'OrchardCore_Microsoft_Authentication_MicrosoftAccount' section is deprecated and will be removed in a future major version, use 'Authentication:MicrosoftAccount' instead.
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>()
                .GetSectionCompat("Authentication:MicrosoftAccount", "OrchardCore_Microsoft_Authentication_MicrosoftAccount");

            tenantServices
                .AddOptions<MicrosoftAccountSettings>()
                .PostConfigure<IDataProtectionProvider, ShellSettings, ILoggerFactory>((settings, dataProtectionProvider, shellSettings, loggerFactory) =>
                {
                    configurationSection.Bind(settings);
                    configurationSection.WarnIfLegacySecretConfigured(
                        loggerFactory.CreateLogger(SecretConfigurationExtensions.LoggerCategory),
                        shellSettings.Name, "Microsoft Account", "AppSecret");

                    // Secrets are consumed protected, as when they are saved from the admin settings, so protect the configured ones.
#pragma warning disable CS0618 // Protect legacy credentials supplied through configuration.
                    var appSecret = configurationSection[nameof(MicrosoftAccountSettings.AppSecret)];

                    if (!string.IsNullOrWhiteSpace(appSecret))
                    {
                        settings.AppSecret = dataProtectionProvider.CreateProtector(MicrosoftAuthenticationConstants.Features.MicrosoftAccount).Protect(appSecret);
                    }
#pragma warning restore CS0618
                });
        });

        return builder;
    }

    public static OrchardCoreBuilder ConfigureAzureADSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configuration = serviceProvider.GetRequiredService<IShellConfiguration>();

            tenantServices.PostConfigure<AzureADSettings>(settings =>
            {
                // The 'OrchardCore_Microsoft_Authentication_AzureAD' section is deprecated and will be removed in a future major version, use 'Authentication:AzureAD' instead.
                configuration.GetSectionCompat("Authentication:AzureAD", "OrchardCore_Microsoft_Authentication_AzureAD").Bind(settings);
            });
        });

        return builder;
    }
}
