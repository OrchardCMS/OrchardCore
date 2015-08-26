using Microsoft.Framework.DependencyInjection;
using Orchard.DependencyInjection;
using Orchard.Hosting;
using Orchard.Hosting.Mvc;

namespace Orchard.Web {
    public class ShellModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddLogging();
            serviceCollection.AddOptions();

            serviceCollection
                .AddOrchardMvc();

            serviceCollection.AddScoped<IOrchardShell, OrchardShell>();
        }
    }
}
