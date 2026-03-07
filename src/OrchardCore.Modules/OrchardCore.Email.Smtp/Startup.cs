using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Email.Services;
using OrchardCore.Email.Smtp.Drivers;
using OrchardCore.Email.Smtp.Services;
using OrchardCore.Modules;

namespace OrchardCore.Email.Smtp;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSiteDisplayDriver<SmtpSettingsDisplayDriver>();

        services.AddTransient<IConfigureOptions<SmtpOptions>, SmtpOptionsConfiguration>();

        services.AddTransient<IEmailProvider, SmtpEmailProvider>();
    }
}
