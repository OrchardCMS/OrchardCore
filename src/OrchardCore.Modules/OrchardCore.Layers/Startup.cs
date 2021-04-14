using System;
using Fluid;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Admin;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
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
using YesSql.Indexes;

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
            services.AddSingleton<IIndexProvider, LayerMetadataIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddRecipeExecutionStep<LayerStep>();

            services.AddTransient<IDeploymentSource, AllLayersDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllLayersDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllLayersDeploymentStepDriver>();
            services.AddSingleton<IGlobalMethodProvider, DefaultLayersMethodProvider>();

            // Registered as part of layers as the controller is shared between features.

            services.AddScoped<IAdminLayerService, AdminLayerService>();
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

    [RequireFeatures("OrchardCore.Deployment")]

    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, AllLayersDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllLayersDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, AllLayersDeploymentStepDriver>();
        } 
    }

    [Feature("OrchardCore.AdminLayers")]

    public class AdminLayersStartup : StartupBase
    {       
        private readonly AdminOptions _adminOptions;

        public AdminLayersStartup(IOptions<AdminOptions> adminOptions)
        {
            _adminOptions = adminOptions.Value;
        }
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(AdminLayerFilter));
            });

            services.AddScoped<IDisplayDriver<ISite>, AdminLayerSiteSettingsDisplayDriver>();
            services.AddContentPart<AdminLayerMetadata>();
            services.AddScoped<IContentDisplayDriver, AdminLayerMetadataWelder>();
            services.AddScoped<INavigationProvider, AdminLayerAdminMenu>();
            services.AddSingleton<IIndexProvider, AdminLayerMetadataIndexProvider>();
            services.AddScoped<IDataMigration, AdminLayerMigrations>();
            services.AddScoped<IPermissionProvider, AdminLayerPermissions>();
            // services.AddRecipeExecutionStep<AdminLayerStep>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var adminControllerName = typeof(AdminController).ControllerName();

            routes.MapAreaControllerRoute(
                name: "AdminLayers.Index",
                areaName: "OrchardCore.Layers",
                pattern: _adminOptions.AdminUrlPrefix + "/AdminLayers",
                defaults: new { controller = adminControllerName, action = nameof(AdminController.Admin) }
            );
        }        
    }

    [RequireFeatures("OrchardCore.Deployment")]

    public class AdminDeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // services.AddTransient<IDeploymentSource, AllLayersDeploymentSource>();
            // services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<AllLayersDeploymentStep>());
            // services.AddScoped<IDisplayDriver<DeploymentStep>, AllLayersDeploymentStepDriver>();
        } 
    }       
}
