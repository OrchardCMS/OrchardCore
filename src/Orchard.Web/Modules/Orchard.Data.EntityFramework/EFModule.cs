using Microsoft.Extensions.DependencyInjection;
using Orchard.DependencyInjection;

namespace Orchard.Data.EntityFramework {
    [OrchardFeature("Orchard.Data.EntityFramework")]
    public class EFModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddScoped<IDbContextFactoryHolder, DbContextFactoryHolder>();
        }
    }
}