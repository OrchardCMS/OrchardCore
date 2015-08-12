using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.DependencyInjection {
    public class DIModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddScoped<IOrchardAssemblyProvider, OrchardAssemblyProvider>();
        }
    }
}