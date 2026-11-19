using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AdminDashboard.Services;
using OrchardCore.Dashboards;
using OrchardCore.Data.Migration;
using OrchardCore.Localization;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminDashboard;

public sealed class Startup : StartupBase
{
    public override int ConfigureOrder => -10;

    private readonly AdminOptions _adminOptions;

    public Startup(IOptions<AdminOptions> adminOptions)
    {
        _adminOptions = adminOptions.Value;
    }

    public override void ConfigureServices(IServiceCollection services)
    {
        services.AddPermissionProvider<Permissions>();
        services.AddBreadcrumbProvider<DashboardBreadcrumbProvider>();

        services.AddDashboardType(AdminDashboardConstants.DashboardType, LocalizationSource.Create<Startup>("Dashboard"), type =>
        {
            type.ViewPermission = Permissions.AccessAdminDashboard;
            type.ManagePermission = Permissions.ManageAdminDashboard;
        });

        // The widgets saved without a dashboard, for example by a recipe, are on the admin dashboard.
        services.Configure<DashboardOptions>(options => options.DefaultType = AdminDashboardConstants.DashboardType);

        services.AddDataMigration<Migrations>();
    }

    public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
    {
        // The admin dashboard is served by the dashboard controller of the Dashboards feature, at the root of the admin.
        const string dashboardControllerName = "Dashboard";

        routes.MapAreaControllerRoute(
            name: "AdminDashboard",
            areaName: DashboardConstants.Features.Dashboards,
            pattern: _adminOptions.AdminUrlPrefix,
            defaults: new { controller = dashboardControllerName, action = "Index", type = AdminDashboardConstants.DashboardType }
        );

        routes.MapAreaControllerRoute(
            name: "AdminDashboardManage",
            areaName: DashboardConstants.Features.Dashboards,
            pattern: $"{_adminOptions.AdminUrlPrefix}/dashboard/manage",
            defaults: new { controller = dashboardControllerName, action = "Manage", type = AdminDashboardConstants.DashboardType }
        );
    }
}
