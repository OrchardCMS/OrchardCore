using Microsoft.Framework.DependencyInjection;
using Orchard.DependencyInjection;

namespace Orchard.Data.EntityFramework {
    public class EFModule : IModule {
        public void Configure(IServiceCollection serviceCollection) {
            serviceCollection.AddEntityFramework()
                .AddInMemoryDatabase()
                .AddDbContext<DataContext>();
        }
    }
}