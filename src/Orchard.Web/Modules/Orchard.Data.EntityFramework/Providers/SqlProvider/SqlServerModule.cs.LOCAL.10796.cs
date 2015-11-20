using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;

namespace Orchard.Data.EntityFramework.Providers.SqlProvider {
    [OrchardFeature("Orchard.Data.EntityFramework.SqlServer")]
    public class SqlServerModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection
                .AddEntityFramework()
                .AddSqlServer()
                .AddDbContext<DataContext>();

            serviceCollection.AddScoped<IContentStoreDataProvider, SqlServerDataServicesProvider>();
            serviceCollection.AddScoped<IDataServicesProvider, SqlServerDataServicesProvider>();
        }
    }
}
