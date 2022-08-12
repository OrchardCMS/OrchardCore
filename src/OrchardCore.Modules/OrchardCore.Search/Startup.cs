using Microsoft.Extensions.DependencyInjection;
using OrchardCore.Deployment;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Routing;
using OrchardCore.Navigation;
using OrchardCore.Search.Drivers;
using OrchardCore.Search.Deployment;
using OrchardCore.Security.Permissions;
using OrchardCore.Settings;
using OrchardCore.Search.Model;
using Microsoft.Extensions.Options;
using OrchardCore.Search.Configuration;

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

            //services.AddScoped<IAreaControllerRouteMapper, SearchAreaControllerRouteMapper>();
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
