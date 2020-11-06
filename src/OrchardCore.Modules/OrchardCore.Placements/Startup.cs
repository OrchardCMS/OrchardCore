using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentTypes.Editors;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Placements.Controllers;
using OrchardCore.Placements.Deployment;
using OrchardCore.Placements.Recipes;
using OrchardCore.Placements.Services;
using OrchardCore.Placements.Settings;
using OrchardCore.Recipes;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Placements
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
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.TryAddScoped<IPlacementStore, DatabasePlacementsStore>();
            services.AddScoped<PlacementsManager>();
            services.AddScoped<IShapePlacementProvider, PlacementProvider>();

            // Shortcuts in settings
            services.AddScoped<IContentPartDefinitionDisplayDriver, PlacementContentPartDefinitionDriver>();
            services.AddScoped<IContentTypePartDefinitionDisplayDriver, PlacementContentTypePartDefinitionDriver>();
            services.AddScoped<IContentPartFieldDefinitionDisplayDriver, PlacementContentPartFieldDefinitionDisplayDriver>();

            // Recipes
            services.AddRecipeExecutionStep<PlacementStep>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var templateControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Placements.Index",
                areaName: "OrchardCore.Placements",
                pattern: _adminOptions.AdminUrlPrefix + "/Placements",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Placements.Create",
                areaName: "OrchardCore.Placements",
                pattern: _adminOptions.AdminUrlPrefix + "/Placements/Create",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Placements.Edit",
                areaName: "OrchardCore.Placements",
                pattern: _adminOptions.AdminUrlPrefix + "/Placements/Edit/{shapeType}",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "Placements.Delete",
                areaName: "OrchardCore.Placements",
                pattern: _adminOptions.AdminUrlPrefix + "/Placements/Delete/{shapeType}",
                defaults: new { controller = templateControllerName, action = nameof(AdminController.Delete) }
            );
        }
    }

    [Feature("OrchardCore.Placements.FileStorage")]
    public class FileContentDefinitionStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.RemoveAll<IPlacementStore>();
            services.AddScoped<IPlacementStore, FilePlacementsStore>();
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, PlacementsDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<PlacementsDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, PlacementsDeploymentStepDriver>();
        }
    }
}
