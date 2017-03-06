using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace Orchard.Environment.Cache
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCaching(this IServiceCollection services)
        {
            services.AddTransient<ITagCache, DefaultTagCache>();
            services.AddSingleton<ISignal, Signal>();
            services.AddScoped<ICacheContextManager, CacheContextManager>();

            // MVC is already registering IMemoryCache as host singleton. We are registering it again
            // in this module so that there is one instance for each tenant.
            // Important: we can't call AddMemoryCache as it's using the TryAdd pattern and hence would
            // not override any existing instance defined at the host level by MVC
            services.AddSingleton<IMemoryCache, MemoryCache>();

            // LocalCache is registered as transient as its implementation resolves IMemoryCache, thus
            // there is no state to keep in its instance.
            services.AddTransient<IDistributedCache, MemoryDistributedCache>();

            return services;
        }
    }
}
