using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Search.Configuration;
using OrchardCore.Search.Deployment;
using OrchardCore.Search.Drivers;
using OrchardCore.Search.Model;
using OrchardCore.Search.Routing;
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
            services.AddTransient<IConfigureOptions<SearchSettings>, SearchSettingsConfiguration>();
            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<ISite>, SearchSettingsDisplayDriver>();
            services.AddScoped<IShapeTableProvider, SearchShapesTableProvider>();
            services.AddShapeAttributes<SearchShapes>();
            services.AddSingleton<SearchRouteTransformer>();
        }

        public override void Configure(IApplicationBuilder builder, IEndpointRouteBuilder routes, IServiceProvider serviceProvider)
        {
            routes.MapDynamicControllerRoute<SearchRouteTransformer>("/search");
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
