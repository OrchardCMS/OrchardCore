using Microsoft.Framework.DependencyInjection;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting;
using OrchardVNext.Hosting.Web.Mvc;
using OrchardVNext.Hosting.Web.Routing.Routes;
using OrchardVNext.Logging;

namespace OrchardVNext.Setup {
    public class SetupMode : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            new HostingModule().Configure(serviceCollection);
            new MvcModule().Configure(serviceCollection);
            new LoggingModule().Configure(serviceCollection);

            serviceCollection.AddScoped<IRoutePublisher, RoutePublisher>();
        }
    }
}
