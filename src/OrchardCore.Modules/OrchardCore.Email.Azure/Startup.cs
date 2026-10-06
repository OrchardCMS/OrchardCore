using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.Azure.Email.Drivers;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Azure.Models;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Services;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Secrets;

namespace OrchardCore.Email.Azure;

public sealed class Startup
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSignalOptionsChangeTokenSource<AzureEmailOptions>();

        services.AddTransient<IConfigureOptions<AzureEmailOptions>, AzureEmailOptionsConfiguration>();

        services.AddEmailProviderOptionsConfiguration<AzureEmailProviderOptionsConfigurations>()
            .AddSiteDisplayDriver<AzureEmailSettingsDisplayDriver>();

        services.AddOptions<DefaultAzureEmailOptions>().Configure<ShellSettings, ILoggerFactory>((options, shellSettings, loggerFactory) =>
        {
            // The 'OrchardCore_Email_Azure' and 'OrchardCore_Email_AzureCommunicationServices' sections are deprecated and will be removed
            // in a future major version, use 'Email:Azure' instead.
            var section = _shellConfiguration
                .GetSectionCompat("Email:Azure", "OrchardCore_Email_Azure", "OrchardCore_Email_AzureCommunicationServices");
            section.Bind(options);
            section.WarnIfLegacySecretConfigured(
                loggerFactory.CreateLogger(SecretConfigurationExtensions.LoggerCategory),
                shellSettings.Name, "Azure Email", "ConnectionString");

            options.IsEnabled = options.ConfigurationExists();
        });

        services.AddScoped<IDataMigration, Migrations>();
    }
}
