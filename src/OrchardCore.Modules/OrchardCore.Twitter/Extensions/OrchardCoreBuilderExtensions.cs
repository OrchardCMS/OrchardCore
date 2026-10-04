using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Twitter;
using OrchardCore.Twitter.Settings;
using OrchardCore.Secrets;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureTwitterSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            // The 'OrchardCore_X' and 'OrchardCore_Twitter' sections are deprecated and will be removed in a future major version, use 'X' instead.
            var configurationSection = serviceProvider.GetRequiredService<IShellConfiguration>()
                .GetSectionCompat("X", "OrchardCore_X", "OrchardCore_Twitter");

            tenantServices
                .AddOptions<TwitterSettings>()
                .PostConfigure<IDataProtectionProvider, ShellSettings, ILoggerFactory>((settings, dataProtectionProvider, shellSettings, loggerFactory) =>
                {
                    configurationSection.Bind(settings);
                    configurationSection.WarnIfLegacySecretConfigured(
                        loggerFactory.CreateLogger(SecretConfigurationExtensions.LoggerCategory),
                        shellSettings.Name, "Twitter", "ConsumerSecret", "AccessTokenSecret");

                    // Secrets are consumed protected, as when they are saved from the admin settings, so protect the configured ones.
                    var protector = dataProtectionProvider.CreateProtector(TwitterConstants.Features.Twitter);
#pragma warning disable CS0618 // Protect legacy credentials supplied through configuration.
                    var consumerSecret = configurationSection[nameof(TwitterSettings.ConsumerSecret)];
                    var accessTokenSecret = configurationSection[nameof(TwitterSettings.AccessTokenSecret)];

                    if (!string.IsNullOrWhiteSpace(consumerSecret))
                    {
                        settings.ConsumerSecret = protector.Protect(consumerSecret);
                    }

                    if (!string.IsNullOrWhiteSpace(accessTokenSecret))
                    {
                        settings.AccessTokenSecret = protector.Protect(accessTokenSecret);
                    }
#pragma warning restore CS0618
                });
        });

        return builder;
    }
}
