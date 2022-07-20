using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.Modules;
using OrchardCore.Mvc.Routing;
using OrchardCore.Navigation;
using OrchardCore.Search.Configuration;
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
            //services.AddTransient<IConfigureOptions<SearchSettings>, SearchSettingsConfiguration>();
            services.AddScoped<IAreaControllerRouteMapper, SearchAreaControllerRouteMapper>();

            services.AddScoped<INavigationProvider, AdminMenu>();
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<IDisplayDriver<ISite>, SearchSettingsDisplayDriver>();
            services.AddScoped<IShapeTableProvider, SearchShapesTableProvider>();
            services.AddShapeAttributes<SearchShapes>();
        }
    }
}
