using System;
using Fluid;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Layers.Controllers;
using OrchardCore.Layers.Deployment;
using OrchardCore.Layers.Drivers;
using OrchardCore.Layers.Handlers;
using OrchardCore.Layers.Indexes;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Recipes;
using OrchardCore.Layers.Services;
using OrchardCore.Layers.ViewModels;
using OrchardCore.Modules;
using OrchardCore.Mvc.Core.Utilities;
using OrchardCore.Navigation;
using OrchardCore.Recipes;
using OrchardCore.Scripting;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Layers
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
            services.Configure<TemplateOptions>(o =>
            {
                o.MemberAccessStrategy.Register<WidgetWrapper>();
            });

            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(LayerFilter));
            });

            services.AddScoped<IDisplayDriver<ISite>, LayerSiteSettingsDisplayDriver>();
            services.AddContentPart<LayerMetadata>();
            services.AddScoped<IContentDisplayDriver, LayerMetadataWelder>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<ILayerService, LayerService>();
            services.AddScoped<IContentHandler, LayerMetadataHandler>();
            services.AddIndexProvider<LayerMetadataIndexProvider>();
            services.AddDataMigration<Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddRecipeExecutionStep<LayerStep>();

            services.AddTransient<IDeploymentSource, AllLayersDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllLayersDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllLayersDeploymentStepDriver>();
            services.AddSingleton<IGlobalMethodProvider, DefaultLayersMethodProvider>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Layers.Index",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/Layers",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Index) }
            );

            routes.MapAreaControllerRoute(
                name: "Layers.Create",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/Layers/Create",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Layers.Delete",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/Layers/Delete",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "Layers.Edit",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/Layers/Edit",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Edit) }
            );

            var layerRuleControllerName = typeof(LayerRuleController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "Layers.Rules.Create",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/Layers/Rules/Create",
                defaults: new { controller = layerRuleControllerName, action = nameof(LayerRuleController.Create) }
            );

            routes.MapAreaControllerRoute(
                name: "Layers.Rules.Delete",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/Layers/Rules/Delete",
                defaults: new { controller = layerRuleControllerName, action = nameof(LayerRuleController.Delete) }
            );

            routes.MapAreaControllerRoute(
                name: "Layers.Rules.Edit",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/Layers/Rules/Edit",
                defaults: new { controller = layerRuleControllerName, action = nameof(LayerRuleController.Edit) }
            );

            routes.MapAreaControllerRoute(
                name: "Layers.Rules.Order",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/Layers/Rules/Order",
                defaults: new { controller = layerRuleControllerName, action = nameof(LayerRuleController.UpdateOrder) }
            );
        }
    }
}
