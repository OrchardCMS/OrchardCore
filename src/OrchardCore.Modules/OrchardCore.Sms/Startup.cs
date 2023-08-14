using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.Admin;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Sms.Activities;
using OrchardCore.Sms.Drivers;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Sms;

public class Startup : StartupBase
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

        // Add Twilio provider.
        services.AddTwilioProvider()
            .AddScoped<IDisplayDriver<ISite>, TwilioSettingsDisplayDriver>();

        if (_hostEnvironment.IsDevelopment())
        {
            // Add Console provider.
            services.AddConsoleProvider();
        }

        services.AddScoped<IPermissionProvider, SmsPermissionProvider>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IDisplayDriver<ISite>, SmsSettingsDisplayDriver>();
    }
}

[RequireFeatures("OrchardCore.Workflows")]
public class WorkflowsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<SmsTask, SmsTaskDisplayDriver>();
    }
}
