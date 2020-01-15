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
    /// Exposes a method allowing to resolve an application store.
    /// </summary>
    public class OpenIdApplicationStoreResolver : IOpenIddictApplicationStoreResolver
    {
        private readonly ConcurrentDictionary<Type, Type> _cache = new ConcurrentDictionary<Type, Type>();
        private readonly IServiceProvider _provider;

        public OpenIdApplicationStoreResolver(IServiceProvider provider)
            => _provider = provider;

        /// <summary>
        /// Returns an application store compatible with the specified application type or throws an
        /// <see cref="InvalidOperationException"/> if no store can be built using the specified type.
        /// </summary>
        /// <typeparam name="TApplication">The type of the Application entity.</typeparam>
        /// <returns>An <see cref="IOpenIddictApplicationStore{TApplication}"/>.</returns>
        public IOpenIddictApplicationStore<TApplication> Get<TApplication>() where TApplication : class
        {
            var store = _provider.GetService<IOpenIddictApplicationStore<TApplication>>();
            if (store != null)
            {
                return store;
            }

            var type = _cache.GetOrAdd(typeof(TApplication), key =>
            {
                if (!typeof(OpenIdApplication).IsAssignableFrom(key))
                {
                    throw new InvalidOperationException(new StringBuilder()
                        .AppendLine("The specified application type is not compatible with the YesSql stores.")
                        .Append("When enabling the YesSql stores, make sure you use the built-in 'OpenIdApplication' ")
                        .Append("entity (from the 'OrchardCore.OpenId.Core' package) or a custom entity ")
                        .Append("that inherits from the 'OpenIdApplication' entity.")
                        .ToString());
                }

                return typeof(OpenIdApplicationStore<>).MakeGenericType(key);
            });

            return (IOpenIddictApplicationStore<TApplication>) _provider.GetRequiredService(type);
        }
    }
}
