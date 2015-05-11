using Microsoft.Framework.DependencyInjection;

namespace OrchardVNext.Data.EF {
    public class EFModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddEntityFramework()
                .AddInMemoryStore()
                .AddDbContext<DataContext>();
        }
    }
}