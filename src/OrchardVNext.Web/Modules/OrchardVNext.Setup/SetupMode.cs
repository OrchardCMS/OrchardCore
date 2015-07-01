using OrchardVNext.Mvc.Routes;
using Microsoft.Framework.DependencyInjection;
using OrchardVNext.Mvc;

namespace OrchardVNext.Setup {
    public class SetupMode : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            new MvcModule().Configure(serviceCollection);

            serviceCollection.AddScoped<IRoutePublisher, RoutePublisher>();
        }
    }
}
