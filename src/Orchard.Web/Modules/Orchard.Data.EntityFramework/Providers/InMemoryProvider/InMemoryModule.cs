using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;

namespace Orchard.Data.EntityFramework.Providers.SqlProvider {
    [OrchardFeature("Orchard.Data.EntityFramework.InMemory")]
    public class InMemoryModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<DataContext>();

            serviceCollection.AddScoped<IContentStoreDataProvider, InMemoryDataServicesProvider>();
            serviceCollection.AddScoped<IDataServicesProvider, InMemoryDataServicesProvider>();
        }
    }
}
