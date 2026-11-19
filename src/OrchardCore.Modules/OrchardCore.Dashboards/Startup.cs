using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentTypes.Events;
using OrchardCore.Dashboards.Drivers;
using OrchardCore.Dashboards.Handlers;
using OrchardCore.Dashboards.Indexes;
using OrchardCore.Dashboards.Models;
using OrchardCore.Dashboards.Security;
using OrchardCore.Dashboards.Services;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Dashboards;

public sealed class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<IDashboardManager, DashboardManager>();
        services.AddIndexProvider<DashboardWidgetIndexProvider>();

        services.AddContentPart<DashboardPart>()
            .UseDisplayDriver<DashboardPartDisplayDriver>()
            .AddHandler<DashboardPartHandler>();

        services.AddScoped<IContentDisplayDriver, DashboardContentDisplayDriver>();
        services.AddScoped<IAuthorizationHandler, UserDashboardWidgetAuthorizationHandler>();

        services.AddDataMigration<Migrations>();
        services.AddScoped<IContentDefinitionHandler, DashboardPartContentTypeDefinitionHandler>();
    }
}

[Feature(DashboardConstants.Features.User)]
public sealed class UserDashboardStartup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<UserDashboardPermissions>();
        services.AddNavigationProvider<UserDashboardAdminMenu>();

        services.AddDashboardType(DashboardConstants.UserDashboardType, UserDashboardPermissions.DisplayName, type =>
        {
            type.Scope = DashboardScope.User;
            type.ManagePermission = UserDashboardPermissions.ManageOwnDashboard;
        });
    }
}
