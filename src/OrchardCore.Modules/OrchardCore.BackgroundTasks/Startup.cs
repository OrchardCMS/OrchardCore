using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.BackgroundTasks.Controllers;
using OrchardCore.BackgroundTasks.Services;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Security.Permissions;

namespace OrchardCore.BackgroundTasks
{
    public class Startup : StartupBase
    {
        private readonly AdminOptions _adminOptions;

        public Startup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services
                .AddScoped<BackgroundTaskManager>()
                .AddScoped<IPermissionProvider, Permissions>()
                .AddScoped<INavigationProvider, AdminMenu>()
                .AddScoped<IBackgroundTaskSettingsProvider, BackgroundTaskSettingsProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var backgroundTaskControllerName = typeof(BackgroundTaskController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "BackgroundTasks",
                areaName: "OrchardCore.BackgroundTasks",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundTasks",
                defaults: new { controller = backgroundTaskControllerName, action = nameof(BackgroundTaskController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "BackgroundTasksEdit",
                areaName: "OrchardCore.BackgroundTasks",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundTasks/Edit/{name}",
                defaults: new { controller = backgroundTaskControllerName, action = nameof(BackgroundTaskController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "BackgroundTasksEnable",
                areaName: "OrchardCore.BackgroundTasks",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundTasks/Enable/{name}",
                defaults: new { controller = backgroundTaskControllerName, action = nameof(BackgroundTaskController.Enable) }
            );

            routes.MapAreaControllerRoute(
                name: "BackgroundTasksDisable",
                areaName: "OrchardCore.BackgroundTasks",
                pattern: _adminOptions.AdminUrlPrefix + "/BackgroundTasks/Disable/{name}",
                defaults: new { controller = backgroundTaskControllerName, action = nameof(BackgroundTaskController.Disable) }
            );
        }
    }
}
