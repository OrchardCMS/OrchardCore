/*
 * Licensed under the Apache License, Version 2.0 (http://www.apache.org/licenses/LICENSE-2.0)
 * See https://github.com/openiddict/openiddict-core for more information concerning
 * the license and the contributors participating to this project.
 */

using System;
using System.Collections.Concurrent;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.YesSql.Models;
using OrchardCore.OpenId.YesSql.Stores;

namespace OrchardCore.OpenId.YesSql.Resolvers
{
    /// <summary>
    /// Exposes a method allowing to resolve an authorization store.
    /// </summary>
    public class OpenIdAuthorizationStoreResolver : IOpenIddictAuthorizationStoreResolver
    {
        private readonly ConcurrentDictionary<Type, Type> _cache = new ConcurrentDictionary<Type, Type>();
        private readonly IServiceProvider _provider;

        public OpenIdAuthorizationStoreResolver(IServiceProvider provider)
            => _provider = provider;

        /// <summary>
        /// Returns an authorization store compatible with the specified authorization type or throws an
        /// <see cref="InvalidOperationException"/> if no store can be built using the specified type.
        /// </summary>
        /// <typeparam name="TAuthorization">The type of the Authorization entity.</typeparam>
        /// <returns>An <see cref="IOpenIddictAuthorizationStore{TAuthorization}"/>.</returns>
        public IOpenIddictAuthorizationStore<TAuthorization> Get<TAuthorization>() where TAuthorization : class
        {
            var store = _provider.GetService<IOpenIddictAuthorizationStore<TAuthorization>>();
            if (store != null)
            {
                return store;
            }

            var type = _cache.GetOrAdd(typeof(TAuthorization), key =>
            {
                if (!typeof(OpenIdAuthorization).IsAssignableFrom(key))
                {
                    throw new InvalidOperationException(new StringBuilder()
                        .AppendLine("The specified authorization type is not compatible with the YesSql stores.")
                        .Append("When enabling the YesSql stores, make sure you use the built-in 'OpenIdAuthorization' ")
                        .Append("entity (from the 'OrchardCore.OpenId.Core' package) or a custom entity ")
                        .Append("that inherits from the 'OpenIdAuthorization' entity.")
                        .ToString());
                }

                return typeof(OpenIdAuthorizationStore<>).MakeGenericType(key);
            });

            return (IOpenIddictAuthorizationStore<TAuthorization>) _provider.GetRequiredService(type);
        }
    }
}
