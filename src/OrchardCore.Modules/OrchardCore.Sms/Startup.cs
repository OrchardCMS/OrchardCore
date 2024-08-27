using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Notifications;
using OrchardCore.Security.Permissions;
using OrchardCore.Sms.Activities;
using OrchardCore.Sms.Drivers;
using OrchardCore.Sms.Services;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Sms;

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
            .AddSiteDisplayDriver<TwilioSettingsDisplayDriver>();

        services.AddPermissionProvider<SmsPermissionProvider>();
        services.AddSiteDisplayDriver<SmsSettingsDisplayDriver>();
        services.AddNavigationProvider<AdminMenu>();
    }
}

[Feature("OrchardCore.Notifications.Sms")]
public sealed class NotificationsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationMethodProvider, SmsNotificationProvider>();
    }
}

[RequireFeatures("OrchardCore.Workflows")]
public sealed class WorkflowsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<SmsTask, SmsTaskDisplayDriver>();
    }
}
