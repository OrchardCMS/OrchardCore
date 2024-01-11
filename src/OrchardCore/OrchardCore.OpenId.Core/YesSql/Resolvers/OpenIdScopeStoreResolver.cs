using System;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.OpenId.YesSql.Stores;

namespace OrchardCore.OpenId.YesSql.Resolvers
{
    /// <inheritdoc/>
    public class OpenIdScopeStoreResolver : IOpenIddictScopeStoreResolver
    {
        private readonly TypeResolutionCache _cache;
        private readonly IServiceProvider _provider;

        public OpenIdScopeStoreResolver(TypeResolutionCache cache, IServiceProvider provider)
        {
            _cache = cache;
            _provider = provider;
        }

        /// <inheritdoc/>
        public IOpenIddictScopeStore<TScope> Get<TScope>() where TScope : class
        {
            var store = _provider.GetService<IOpenIddictScopeStore<TScope>>();
            if (store != null)
            {
                return store;
            }

            var type = _cache.GetOrAdd(typeof(TScope), key =>
            {
                if (!typeof(OpenIdScope).IsAssignableFrom(key))
                {
                    throw new InvalidOperationException(new StringBuilder()
                        .AppendLine("The specified scope type is not compatible with the YesSql stores.")
                        .Append("When enabling the YesSql stores, make sure you use the built-in 'OpenIdScope' ")
                        .Append("entity (from the 'OrchardCore.OpenId.Core' package) or a custom entity ")
                        .Append("that inherits from the 'OpenIdScope' entity.")
                        .ToString());
                }

                return typeof(OpenIdScopeStore<>).MakeGenericType(key);
            });

            return (IOpenIddictScopeStore<TScope>)_provider.GetRequiredService(type);
        }

        // Note: OrchardCore YesSql resolvers are registered as scoped dependencies as their inner
        // service provider must be able to resolve scoped services (typically, the store they return).
        // To avoid having to declare a static type resolution cache, a special cache service is used
        // here and registered as a singleton dependency so that its content persists beyond the scope.
        public class TypeResolutionCache : ConcurrentDictionary<Type, Type> { }
    }
}
