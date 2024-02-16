using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Azure.Services;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Settings;

namespace OrchardCore.Email.Smtp;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSmtpEmailProvider()
            .AddScoped<IDisplayDriver<ISite>, SmtpSettingsDisplayDriver>()
            .AddTransient<IConfigureOptions<SmtpOptions>, SmtpOptionsConfiguration>();
    }
}
