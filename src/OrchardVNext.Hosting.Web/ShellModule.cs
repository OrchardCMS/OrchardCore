using Microsoft.Framework.DependencyInjection;
using OrchardVNext.DependencyInjection;
using OrchardVNext.Hosting;
using OrchardVNext.Hosting.Mvc;

namespace OrchardVNext.Web {
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
