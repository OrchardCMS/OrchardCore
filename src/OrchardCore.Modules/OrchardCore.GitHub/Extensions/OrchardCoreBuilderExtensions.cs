using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.GitHub.Settings;
using OrchardCore.Secrets;

namespace Microsoft.Extensions.DependencyInjection;

public static class OrchardCoreBuilderExtensions
{
    public static OrchardCoreBuilder ConfigureGitHubSettings(this OrchardCoreBuilder builder)
    {
        builder.ConfigureServices((tenantServices, serviceProvider) =>
        {
            var configuration = serviceProvider.GetRequiredService<IShellConfiguration>();

            tenantServices.AddOptions<GitHubAuthenticationSettings>()
                .PostConfigure<ShellSettings, ILoggerFactory>((settings, shellSettings, loggerFactory) =>
                {
                    // The 'OrchardCore_GitHub' section is deprecated and will be removed in a future major version, use 'Authentication:GitHub' instead.
                    var section = configuration.GetSectionCompat("Authentication:GitHub", "OrchardCore_GitHub");
                    section.Bind(settings);
                    section.WarnIfLegacySecretConfigured(
                        loggerFactory.CreateLogger(SecretConfigurationExtensions.LoggerCategory),
                        shellSettings.Name, "GitHub", "ClientSecret");
                });
        });

        return builder;
    }
}
