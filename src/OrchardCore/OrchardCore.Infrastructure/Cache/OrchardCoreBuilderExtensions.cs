using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using OrchardCore.Environment.Cache;
using OrchardCore.Environment.Cache.CacheContextProviders;
using OrchardCore.Infrastructure.Cache;

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class OrchardCoreBuilderExtensions
    {
        /// <summary>
        /// Adds tenant level caching services.
        /// </summary>
        public static OrchardCoreBuilder AddCaching(this OrchardCoreBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddScoped<ITagCache, DefaultTagCache>();
                services.AddSingleton<ISignal, Signal>();
                services.AddScoped<ICacheContextManager, CacheContextManager>();
                services.AddScoped<ICacheScopeManager, CacheScopeManager>();

                services.AddScoped<ICacheContextProvider, FeaturesCacheContextProvider>();
                services.AddScoped<ICacheContextProvider, QueryCacheContextProvider>();
                services.AddScoped<ICacheContextProvider, RolesCacheContextProvider>();
                services.AddScoped<ICacheContextProvider, RouteCacheContextProvider>();
                services.AddScoped<ICacheContextProvider, UserCacheContextProvider>();
                services.AddScoped<ICacheContextProvider, KnownValueCacheContextProvider>();

                // IMemoryCache is registered at the tenant level so that there is one instance for each tenant.
                services.AddSingleton<IMemoryCache, MemoryCache>();

                // MemoryDistributedCache needs to be registered as a singleton as it owns a MemoryCache instance.
                services.AddSingleton<IDistributedCache, MemoryDistributedCache>();

                // Provides a distributed cache service that can return existing references in the current scope.
                services.AddScoped<IScopedDistributedCache, ScopedDistributedCache>();
            });

            return builder;
        }
    }
}
