using Orchard.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;

namespace Orchard.Environment.Cache
{
    public class DefaultMemoryCache : IModule
    {
        public void Configure(IServiceCollection serviceCollection)
        {
            serviceCollection.AddInstance<IMemoryCache>(
                new MemoryCache(new MemoryCacheOptions()
                {
                    CompactOnMemoryPressure = false,
                })
            );
        }
    }
}
