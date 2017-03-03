using Microsoft.AspNetCore.Modules;
using Microsoft.Extensions.DependencyInjection;
using Orchard.DisplayManagement.Descriptors;
using Orchard.Environment.Navigation;

namespace Orchard.Navigation
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
