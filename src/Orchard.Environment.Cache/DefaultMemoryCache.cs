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
            // NB: There seems to be an already registered IMemoryCache as removing this line 
            // will still resolve one. IDistributedCache is definitely not registered withou it.
            serviceCollection.Add(ServiceDescriptor.Singleton<IMemoryCache, MemoryCache>());
            serviceCollection.Add(ServiceDescriptor.Transient<IDistributedCache, LocalCache>());
        }
    }
}
