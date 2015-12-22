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
            // MVC is already registering IMemoryCache. Any module that would add another implementation
            // would take over the default one as the last registered service will be resolved.
            // serviceCollection.Add(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());


            // LocalCache is registered as Transient as its implementation resolves IMemoryCache, thus
            // there is no state to keep in its instance.
            serviceCollection.Add(ServiceDescriptor.Transient<IDistributedCache, LocalCache>());
        }
    }
}
