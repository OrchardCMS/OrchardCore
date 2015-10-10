using Microsoft.Extensions.DependencyInjection;
using Orchard.Data;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Events;
using Orchard.Hosting.Mvc;

namespace Orchard.Hosting {
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
