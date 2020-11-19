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
    public class OpenIdTokenStoreResolver : IOpenIddictTokenStoreResolver
    {
        private readonly TypeResolutionCache _cache;
        private readonly IServiceProvider _provider;

        public OpenIdTokenStoreResolver(TypeResolutionCache cache, IServiceProvider provider)
        {
            _cache = cache;
            _provider = provider;
        }

        /// <inheritdoc/>
        public IOpenIddictTokenStore<TToken> Get<TToken>() where TToken : class
        {
            var store = _provider.GetService<IOpenIddictTokenStore<TToken>>();
            if (store != null)
            {
                return store;
            }

            var type = _cache.GetOrAdd(typeof(TToken), key =>
            {
                if (!typeof(OpenIdToken).IsAssignableFrom(key))
                {
                    throw new InvalidOperationException(new StringBuilder()
                        .AppendLine("The specified token type is not compatible with the YesSql stores.")
                        .Append("When enabling the YesSql stores, make sure you use the built-in 'OpenIdToken' ")
                        .Append("entity (from the 'OrchardCore.OpenId.Core' package) or a custom entity ")
                        .Append("that inherits from the 'OpenIdToken' entity.")
                        .ToString());
                }

                return typeof(OpenIdTokenStore<>).MakeGenericType(key);
            });

            return (IOpenIddictTokenStore<TToken>)_provider.GetRequiredService(type);
        }

        // Note: OrchardCore YesSql resolvers are registered as scoped dependencies as their inner
        // service provider must be able to resolve scoped services (typically, the store they return).
        // To avoid having to declare a static type resolution cache, a special cache service is used
        // here and registered as a singleton dependency so that its content persists beyond the scope.
        public class TypeResolutionCache : ConcurrentDictionary<Type, Type> { }
    }
}
