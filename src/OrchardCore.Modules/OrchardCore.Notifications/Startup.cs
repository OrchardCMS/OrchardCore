using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Data.Migration;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Notifications.Activities;
using OrchardCore.Notifications.Drivers;
using OrchardCore.Notifications.Migrations;
using OrchardCore.Notifications.Services;
using OrchardCore.Users.Models;
using OrchardCore.Workflows.Helpers;

namespace OrchardCore.Notifications;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationManager, NotificationManager>();
        services.AddScoped<IDataMigration, UserMigrations>();
        services.AddScoped<IDisplayDriver<User>, UserNotificationPartDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications")]
[RequireFeatures("OrchardCore.Workflows")]
public class WorkflowsStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<NotifyUserTask, NotifyUserTaskDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications")]
[RequireFeatures("OrchardCore.Workflows", "OrchardCore.Users")]
public class UsersStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddActivity<NotifyContentOwnerTask, NotifyContentOwnerTaskDisplayDriver>();
    }
}

[Feature("OrchardCore.Notifications.Email")]
public class EmailNotificationStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<INotificationMethodProvider, EmailNotificationProvider>();
    }
}
