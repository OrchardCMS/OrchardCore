using System;
using Microsoft.AspNetCore.Builder;
using OrchardCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement;
using OrchardCore.ContentManagement.Display.ContentDisplay;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Data.Migration;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Navigation;
using OrchardCore.Layers.Deployment;
using OrchardCore.Layers.Drivers;
using OrchardCore.Layers.Handlers;
using OrchardCore.Layers.Indexes;
using OrchardCore.Layers.Models;
using OrchardCore.Layers.Recipes;
using OrchardCore.Layers.Services;
using OrchardCore.Recipes;
using OrchardCore.Scripting;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using YesSql.Indexes;

namespace OrchardCore.Layers
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(LayerFilter));
            });

            services.AddScoped<IDisplayDriver<ISite>, LayerSiteSettingsDisplayDriver>();
            services.AddSingleton<ContentPart, LayerMetadata>();
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
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var scriptingManager = serviceProvider.GetRequiredService<IScriptingManager>();
            scriptingManager.GlobalMethodProviders.Add(new DefaultLayersMethodProvider());
        }
    }
}
