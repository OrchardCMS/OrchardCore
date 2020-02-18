using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Modules;

namespace OrchardCore.Navigation
{
    public class Startup : StartupBase
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddNavigation();

            services.AddScoped<IShapeTableProvider, NavigationShapes>();
            services.AddScoped<IShapeTableProvider, PagerShapesTableProvider>();
            services.AddShapeAttributes<PagerShapes>();
        }
    }
}
