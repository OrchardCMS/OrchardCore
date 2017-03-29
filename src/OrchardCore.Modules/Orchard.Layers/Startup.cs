using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Modules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.ContentManagement;
using Orchard.ContentManagement.Display.ContentDisplay;
using Orchard.ContentManagement.Handlers;
using Orchard.Data.Migration;
using Orchard.Environment.Navigation;
using Orchard.Layers.Drivers;
using Orchard.Layers.Handlers;
using Orchard.Layers.Indexes;
using Orchard.Layers.Models;
using Orchard.Layers.Recipes;
using Orchard.Layers.Services;
using Orchard.Recipes;
using Orchard.Scripting;
using Orchard.Security.Permissions;
using Orchard.Settings.Services;
using YesSql.Core.Indexes;

namespace Orchard.Layers
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.Configure<MvcOptions>((options) =>
            {
                options.Filters.Add(typeof(LayerFilter));
            });

            services.AddScoped<ISiteSettingsDisplayDriver, LayerSiteSettingsDisplayDriver>();
            services.AddSingleton<ContentPart, LayerMetadata>();
            services.AddScoped<IContentDisplayDriver, LayerMetadataWelder>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<ILayerService, LayerService>();
            services.AddScoped<IContentHandler, LayerMetadataHandler>();
            services.AddScoped<IIndexProvider, LayerMetadataIndexProvider>();
            services.AddScoped<IDataMigration, Migrations>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddRecipeExecutionStep<LayerStep>();
        }

        public override void Configure(IApplicationBuilder app, IRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var scriptingManager = serviceProvider.GetRequiredService<IScriptingManager>();
            scriptingManager.GlobalMethodProviders.Add(new DefaultLayersMethodProvider());
        }
    }
}
