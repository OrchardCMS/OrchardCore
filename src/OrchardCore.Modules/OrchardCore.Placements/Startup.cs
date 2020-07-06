using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;
using OrchardCore.Navigation;
using OrchardCore.Placements.Services;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Placements
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IPermissionProvider, Permissions>();
            services.AddScoped<INavigationProvider, AdminMenu>();

            services.AddSingleton<IPlacementFileStore, PlacementFileStore>();
            services.AddScoped<IPlacementRulesService, PlacementRulesService>();
            services.AddScoped<IShapeTableProvider, PlacementProvider>();
        }
    }
}
