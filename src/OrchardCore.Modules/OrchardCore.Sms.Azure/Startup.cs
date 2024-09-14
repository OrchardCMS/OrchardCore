using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Drivers;

namespace OrchardCore.Sms.Azure;

public sealed class Startup : StartupBase
{
    private readonly IHostEnvironment _hostEnvironment;

    public Startup(IHostEnvironment hostEnvironment)
    {
        _hostEnvironment = hostEnvironment;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddSmsServices();
        services.AddPhoneFormatValidator();

        if (_hostEnvironment.IsDevelopment())
        {
            services.AddLogSmsProvider();
        }

        services.AddTwilioSmsProvider()
            .AddSiteSettingsDisplayDriver<AzureSettingsDisplayDriver>();

        services.AddScoped<IPermissionProvider, AzureSmsPermissionProvider>();
    }
}
