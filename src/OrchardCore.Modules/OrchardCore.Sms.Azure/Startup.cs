using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Secrets;
using OrchardCore.Sms.Azure.Drivers;
using OrchardCore.Sms.Azure.Models;

namespace OrchardCore.Sms.Azure;

public sealed class Startup : StartupBase
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddAzureSmsProvider()
            .AddSiteDisplayDriver<AzureSettingsDisplayDriver>();

        services.AddOptions<DefaultAzureSmsOptions>().Configure<ShellSettings, ILoggerFactory>((options, shellSettings, loggerFactory) =>
        {
            // The 'OrchardCore_Sms_AzureCommunicationServices' section is deprecated and will be removed in a future major version, use 'Sms:Azure' instead.
            var section = _shellConfiguration.GetSectionCompat("Sms:Azure", "OrchardCore_Sms_AzureCommunicationServices");
            section.Bind(options);
            section.WarnIfLegacySecretConfigured(
                loggerFactory.CreateLogger(SecretConfigurationExtensions.LoggerCategory),
                shellSettings.Name, "Azure SMS", "ConnectionString");

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}

[RequireFeatures("OrchardCore.Secrets")]
public sealed class SecretsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<SecretMigration, AzureSmsSecretMigrationDisplayDriver>();
    }
}
