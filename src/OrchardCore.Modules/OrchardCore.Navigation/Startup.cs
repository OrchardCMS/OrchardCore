using Microsoft.Extensions.DependencyInjection;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Environment.Shell.Configuration;
using OrchardCore.Modules;

namespace OrchardCore.Navigation
{
    public class Startup : StartupBase
    {
        private readonly IShellConfiguration _shellConfiguration;

        public Startup(IShellConfiguration shellConfiguration)
        {
            _shellConfiguration = shellConfiguration;
        }

        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddNavigation();

            services.AddScoped<IShapeTableProvider, NavigationShapes>();
            services.AddScoped<IShapeTableProvider, PagerShapesTableProvider>();
            services.AddShapeAttributes<PagerShapes>();

            var navigationConfiguration = _shellConfiguration.GetSection("OrchardCore_Navigation");
            services.Configure<PagerOptions>(navigationConfiguration.GetSection("PagerOptions"));
        }
    }
}
