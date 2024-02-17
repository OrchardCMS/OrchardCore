using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp;

public class Startup
{
    private readonly IShellConfiguration _shellConfiguration;

    public Startup(IShellConfiguration shellConfiguration)
    {
        _shellConfiguration = shellConfiguration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSmtpEmailProvider()
            .AddScoped<IDisplayDriver<ISite>, SmtpSettingsDisplayDriver>()
            .AddTransient<IConfigureOptions<SmtpOptions>, SmtpOptionsConfiguration>();

        services.Configure<DefaultSmtpOptions>(options =>
            {
                _shellConfiguration.GetSection("OrchardCore_Email").Bind(options);

                options.IsEnabled = options.ConfigurationExists();
            });
    }
}
