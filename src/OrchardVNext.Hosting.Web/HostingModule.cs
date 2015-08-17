using Microsoft.Framework.DependencyInjection;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Hosting {
    public class HostingModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddScoped<IOrchardShell, OrchardShell>();
        }
    }
}