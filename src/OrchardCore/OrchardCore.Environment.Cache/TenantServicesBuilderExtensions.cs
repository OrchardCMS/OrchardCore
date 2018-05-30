using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Cache.CacheContextProviders;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class TenantServicesBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level caching services.
        /// </summary>
        public static TenantServicesBuilder AddCaching(this TenantServicesBuilder tenant)
        {
            var services = tenant.Services;

            services.AddTransient<ITagCache, DefaultTagCache>();
            services.AddSingleton<ISignal, Signal>();
            services.AddScoped<ICacheContextManager, CacheContextManager>();
            services.AddScoped<ICacheScopeManager, CacheScopeManager>();

            services.AddScoped<ICacheContextProvider, FeaturesCacheContextProvider>();
            services.AddScoped<ICacheContextProvider, QueryCacheContextProvider>();
            services.AddScoped<ICacheContextProvider, RolesCacheContextProvider>();
            services.AddScoped<ICacheContextProvider, RouteCacheContextProvider>();
            services.AddScoped<ICacheContextProvider, UserCacheContextProvider>();
            services.AddScoped<ICacheContextProvider, KnownValueCacheContextProvider>();

            // MVC is already registering IMemoryCache as host singleton. We are registering it again
            // in this module so that there is one instance for each tenant.
            // Important: we can't call AddMemoryCache as it's using the TryAdd pattern and hence would
            // not override any existing instance defined at the host level by MVC
            services.AddSingleton<IMemoryCache, MemoryCache>();

            // MemoryDistributedCache needs to be registered as a singleton as it owns a MemoryCache instance.
            services.AddSingleton<IDistributedCache, MemoryDistributedCache>();

            return tenant;
        }
    }
}