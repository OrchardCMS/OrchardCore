using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Notifications;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Sms.Azure.Activities;
using OrchardCore.Sms.Azure.Drivers;
using OrchardCore.Sms.Services;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Sms.Azure;

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

        if (_hostEnvironment.IsDevelopment())
        {
            services.AddLogSmsProvider();
        }

        services.AddTwilioSmsProvider()
            .AddScoped<IDisplayDriver<ISite>, AzureSettingsDisplayDriver>();

        services.AddScoped<IPermissionProvider, SmsPermissionProvider>();
        services.AddScoped<INavigationProvider, AdminMenu>();
        services.AddScoped<IDisplayDriver<ISite>, SmsSettingsDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications.Sms")]
public class NotificationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationMethodProvider, SmsNotificationProvider>();
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
