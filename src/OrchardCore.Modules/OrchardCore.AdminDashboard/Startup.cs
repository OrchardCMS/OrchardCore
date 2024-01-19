using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.AdminDashboard.Controllers;
using OrchardCore.AdminDashboard.Drivers;
using OrchardCore.AdminDashboard.Indexes;
using OrchardCore.AdminDashboard.Models;
using OrchardCore.AdminDashboard.Services;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Security.Permissions;

namespace OrchardCore.AdminDashboard
{
    public class Startup : StartupBase
    {
        public override int ConfigureOrder => -10;

        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();

            services.AddScoped<IAdminDashboardService, AdminDashboardService>();
            services.AddIndexProvider<DashboardPartIndexProvider>();

            services.AddContentPart<DashboardPart>()
                .UseDisplayDriver<DashboardPartDisplayDriver>();

            services.AddScoped<IContentDisplayDriver, DashboardContentDisplayDriver>();

            services.AddDataMigration<Migrations>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            // Dashboard
            var dashboardControllerName = typeof(DashboardController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "AdminDashboard",
                areaName: "OrchardCore.AdminDashboard",
                pattern: _adminOptions.AdminUrlPrefix,
                defaults: new { controller = dashboardControllerName, action = nameof(DashboardController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "AdminDashboard",
                areaName: "OrchardCore.AdminDashboard",
                pattern: $"{_adminOptions.AdminUrlPrefix}/dashboard/manage",
                defaults: new { controller = dashboardControllerName, action = nameof(DashboardController.Manage) }
            );
        }
    }
}
