using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Email.Smtp.Extensions;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Shell.Configuration;

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
            .AddTransient<IConfigureOptions<SmtpOptions>, SmtpOptionsConfiguration>();

        services.Configure<DefaultSmtpOptions>(options =>
        {
            // To ensure backward compatibility, we will try to associate SMTP settings from multiple sections.
            // The 'OrchardCore_Email' section will be phased out in an upcoming release.
            _shellConfiguration.GetSection("OrchardCore_Email").Bind(options);
            _shellConfiguration.GetSection("OrchardCore_Email_Smtp").Bind(options);

            options.IsEnabled = options.ConfigurationExists();
        });
    }
}
