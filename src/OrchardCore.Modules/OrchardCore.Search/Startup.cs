using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Entities;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Search.Configuration;
using OrchardCore.Search.Deployment;
using OrchardCore.Search.Drivers;
using OrchardCore.Search.Model;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;

namespace OrchardCore.Search
{
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            //services.AddTransient<IAreaControllerRouteMapper, SearchAreaControllerRouteMapper>();
            services.AddTransient<IConfigureOptions<SearchSettings>, SearchSettingsConfiguration>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<ISite>, SearchSettingsDisplayDriver>();
            services.AddScoped<IShapeTableProvider, SearchShapesTableProvider>();
            services.AddShapeAttributes<SearchShapes>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            var site = serviceProvider.GetRequiredService<ISiteService>();
            var settings = site.GetSiteSettingsAsync()
                .GetAwaiter().GetResult()
                .As<SearchSettings>();

            if (!String.IsNullOrEmpty(settings.SearchProvider))
            {
                routes.MapAreaControllerRoute(
                    name: "Search",
                    areaName: "OrchardCore.Search." + settings.SearchProvider,
                    pattern: "Search",
                    defaults: new { controller = "Search", action = "Search" }
                );
            }
        }
    }

    [RequireFeatures("OrchardCore.Deployment")]
    public class DeploymentStartup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IDeploymentSource, SearchSettingsDeploymentSource>();
            services.AddSingleton<IDeploymentStepFactory>(new DeploymentStepFactory<SearchSettingsDeploymentStep>());
            services.AddScoped<IDisplayDriver<DeploymentStep>, SearchSettingsDeploymentStepDriver>();
        }
    }
}
