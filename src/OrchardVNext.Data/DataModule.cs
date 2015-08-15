using Microsoft.Framework.DependencyInjection;
using OrchardVNext.DependencyInjection;

namespace OrchardVNext.Data {
    public class DataModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddScoped<IOrchardDataAssemblyProvider, OrchardDataAssemblyProvider>();
        }
    }
}