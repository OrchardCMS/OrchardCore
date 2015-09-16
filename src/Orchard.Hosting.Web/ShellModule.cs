using Microsoft.Framework.DependencyInjection;
using Orchard.Data;
using Orchard.DependencyInjection;
using Orchard.Events;
using Orchard.Hosting;
using Orchard.Hosting.Mvc;

namespace Orchard.Web {
    public class ShellModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddLogging();
            serviceCollection.AddOptions();
            serviceCollection.AddNotifierEvents();
            
            serviceCollection.AddOrchardMvc();
            serviceCollection.AddDataAccess();

            serviceCollection.AddScoped<IOrchardShell, OrchardShell>();
        }
    }
}
