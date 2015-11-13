using Microsoft.AspNet.Routing;
using Microsoft.Extensions.DependencyInjection;
using Orchard.Data;
using Orchard.DependencyInjection;
using Orchard.Environment.Shell;
using Orchard.Events;
using Orchard.Hosting.Mvc;
using Orchard.Hosting.Mvc.Routing;

namespace Orchard.Hosting {
    /// <summary>
    /// These services are registered on the tenant service collection
    /// </summary>
    public class ShellModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddLogging();
            serviceCollection.AddOptions();
            
            serviceCollection.AddOrchardMvc();
            serviceCollection.AddDataAccess();

            serviceCollection.AddSingleton<IOrchardShell, OrchardShell>();
            serviceCollection.AddSingleton<IRouteBuilder, DefaultShellRouteBuilder>();
        }
    }
}
