using Microsoft.Framework.DependencyInjection;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting;
using OrchardVNext.Hosting.Web.Routing.Routes;
using OrchardVNext.Web;

namespace OrchardVNext.Setup {
    public class SetupMode : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            new HostingModule().Configure(serviceCollection);
            new ShellStartup().ConfigureServices(serviceCollection);

            serviceCollection.AddScoped<IRoutePublisher, RoutePublisher>();
        }
    }
}
