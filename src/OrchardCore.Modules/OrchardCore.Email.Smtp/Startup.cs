using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Email.Smtp.Extensions;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Options;
using OrchardCore.Environment.Shell;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;
using OrchardCore.Secrets;

namespace OrchardCore.Email.Smtp;

public sealed class Startup
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSmtpEmailProvider()
            .AddSiteDisplayDriver<SmtpSettingsDisplayDriver>()
            .AddSignalOptionsChangeTokenSource<SmtpOptions>()
            .AddTransient<IConfigureOptions<SmtpOptions>, SmtpOptionsConfiguration>()
            .AddTransient<IPostConfigureOptions<DefaultSmtpOptions>, DefaultSmtpOptionsConfiguration>();

        services.AddOptions<DefaultSmtpOptions>().Configure<ShellSettings, ILoggerFactory>((options, shellSettings, loggerFactory) =>
        {
            // The 'OrchardCore_Email_Smtp' and 'OrchardCore_Email' sections are deprecated and will be removed in a future major version,
            // use 'Email:Smtp' instead.
            var section = _shellConfiguration.GetSectionCompat("Email:Smtp", "OrchardCore_Email_Smtp", "OrchardCore_Email");
            section.Bind(options);
            section.WarnIfLegacySecretConfigured(
                loggerFactory.CreateLogger(SecretConfigurationExtensions.LoggerCategory),
                shellSettings.Name, "SMTP", "Password");

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}

[RequireFeatures("OrchardCore.Secrets")]
public sealed class SecretsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddDisplayDriver<SecretMigration, SmtpSecretMigrationDisplayDriver>();
    }
}
