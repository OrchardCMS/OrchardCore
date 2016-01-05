using Orchard.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Orchard.Environment.Cache
{
    public class DefaultMemoryCache : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            // MVC is already registering IMemoryCache as host singleton. We are registering it again
            // in this module so that there is one instance for each tenant.
            serviceCollection.Add(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());


            // LocalCache is registered as transient as its implementation resolves IMemoryCache, thus
            // there is no state to keep in its instance.
            serviceCollection.Add(ServiceDescriptor.Transient<IDistributedCache, LocalCache>());
        }
    }
}
