using OrchardVNext.Mvc.Routes;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Logging;
using OrchardVNext.Mvc;

namespace OrchardVNext.Setup {
    public class SetupMode : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            new MvcModule().Configure(serviceCollection);
            new LoggingModule().Configure(serviceCollection);

            serviceCollection.AddScoped<IRoutePublisher, RoutePublisher>();
        }
    }
}
