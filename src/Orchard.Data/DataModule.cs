using Microsoft.Framework.DependencyInjection;
using Orchard.DependencyInjection;

namespace Orchard.Data {
    public class DataModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddScoped<IOrchardDataAssemblyProvider, OrchardDataAssemblyProvider>();
        }
    }
}