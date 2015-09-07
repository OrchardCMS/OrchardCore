using Microsoft.Framework.DependencyInjection;
using Orchard.Data.EntityFramework.Providers;
using Orchard.DependencyInjection;

namespace Orchard.Data.EntityFramework {
    public class EFModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection
                .AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<DataContext>();

            serviceCollection.AddScoped<IDataServicesProvider, SqlServerDataServicesProvider>();
            serviceCollection.AddScoped<IDataServicesProvider, InMemoryDataServicesProvider>();
            serviceCollection.AddScoped<IDbContextFactoryHolder, DbContextFactoryHolder>();
            serviceCollection.AddScoped<IDataServicesProviderFactory, DataServicesProviderFactory>();
        }
    }
}