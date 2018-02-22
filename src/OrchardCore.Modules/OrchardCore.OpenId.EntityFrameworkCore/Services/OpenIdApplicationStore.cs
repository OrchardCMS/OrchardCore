using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.EntityFrameworkCore.Models;
using OrchardCore.OpenId.Services;

namespace OrchardCore.OpenId.EntityFrameworkCore.Services
{
    public class OpenIdApplicationStore<TContext, TKey> : OpenIdApplicationStore<OpenIdApplication<TKey>,
                                                                                 OpenIdAuthorization<TKey>,
                                                                                 OpenIdToken<TKey>, TContext, TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdApplicationStore(TContext context, IMemoryCache cache)
            : base(context, cache)
        {
        }
    }

    public class OpenIdApplicationStore<TApplication, TAuthorization, TToken, TContext, TKey> :
        OpenIddictApplicationStore<TApplication, TAuthorization, TToken, TContext, TKey>, IOpenIdApplicationStore
        where TApplication : OpenIdApplication<TKey, TAuthorization, TToken>, new()
        where TAuthorization : OpenIdAuthorization<TKey, TApplication, TToken>, new()
        where TToken : OpenIdToken<TKey, TApplication, TAuthorization>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdApplicationStore(TContext context, IMemoryCache cache)
            : base(context, cache)
        {
        }

        /// <summary>
        /// Retrieves an application using its physical identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client application corresponding to the identifier.
        /// </returns>
        public virtual Task<TApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base FindByIdAsync() method is called.
            => FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves the physical identifier associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the application.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(TApplication application, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync(application, cancellationToken);

        public virtual async Task<ImmutableArray<string>> GetRolesAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var properties = await GetPropertiesAsync(application, cancellationToken);
            if (properties.TryGetValue(OpenIdConstants.Properties.Roles, StringComparison.OrdinalIgnoreCase, out JToken value))
            {
                return value.ToObject<string[]>().ToImmutableArray();
            }

            return ImmutableArray.Create<string>();
        }

        public virtual async Task<ImmutableArray<TApplication>> ListInRoleAsync(string role, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            // To optimize the efficiency of the query a bit, only applications whose stringified
            // Properties column contains the specified role are returned. Once the applications
            // are retrieved, a second pass is made to ensure only valid elements are returned.
            // Implementers that use this method in a hot path may want to override this method
            // to use SQL Server 2016 functions like JSON_VALUE to make the query more efficient.
            IQueryable<TApplication> Query(IQueryable<TApplication> applications, string state)
                => from application in applications
                   where application.Properties.Contains(state)
                   select application;

            var builder = ImmutableArray.CreateBuilder<TApplication>();

            foreach (var application in await ListAsync((applications, state) => Query(applications, state), role, cancellationToken))
            {
                var roles = await GetRolesAsync(application, cancellationToken);
                if (roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                {
                    builder.Add(application);
                }
            }

            return builder.ToImmutable();
        }

        public virtual async Task SetRolesAsync(TApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var properties = await GetPropertiesAsync(application, cancellationToken);
            properties[OpenIdConstants.Properties.Roles] = new JArray(roles.ToArray());

            await SetPropertiesAsync(application, properties, cancellationToken);
        }

        // Note: the following methods are deliberately implemented as explicit methods so they are not
        // exposed by Intellisense. Their logic MUST be limited to dealing with casts and downcasts.
        // Developers who need to customize the logic SHOULD override the methods taking concretes types.

        // -------------------------------------------------------------
        // Methods defined by the IOpenIddictApplicationStore interface:
        // -------------------------------------------------------------

        Task<long> IOpenIddictApplicationStore<IOpenIdApplication>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        Task<long> IOpenIddictApplicationStore<IOpenIdApplication>.CountAsync<TResult>(Func<IQueryable<IOpenIdApplication>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.CreateAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => CreateAsync((TApplication) application, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.DeleteAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => DeleteAsync((TApplication) application, cancellationToken);

        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByClientIdAsync(identifier, cancellationToken);

        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.FindByPostLogoutRedirectUriAsync(string address, CancellationToken cancellationToken)
            => (await FindByPostLogoutRedirectUriAsync(address, cancellationToken)).CastArray<IOpenIdApplication>();

        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.FindByRedirectUriAsync(string address, CancellationToken cancellationToken)
            => (await FindByRedirectUriAsync(address, cancellationToken)).CastArray<IOpenIdApplication>();

        Task<TResult> IOpenIddictApplicationStore<IOpenIdApplication>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientIdAsync((TApplication) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientSecretAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientSecretAsync((TApplication) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientTypeAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientTypeAsync((TApplication) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetConsentTypeAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetConsentTypeAsync((TApplication) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetDisplayNameAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetDisplayNameAsync((TApplication) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetIdAsync((TApplication) application, cancellationToken);

        Task<ImmutableArray<string>> IOpenIddictApplicationStore<IOpenIdApplication>.GetPermissionsAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetPermissionsAsync((TApplication) application, cancellationToken);

        Task<ImmutableArray<string>> IOpenIddictApplicationStore<IOpenIdApplication>.GetPostLogoutRedirectUrisAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetPostLogoutRedirectUrisAsync((TApplication) application, cancellationToken);

        Task<JObject> IOpenIddictApplicationStore<IOpenIdApplication>.GetPropertiesAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetPropertiesAsync((TApplication) application, cancellationToken);

        Task<ImmutableArray<string>> IOpenIddictApplicationStore<IOpenIdApplication>.GetRedirectUrisAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetRedirectUrisAsync((TApplication) application, cancellationToken);

        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdApplication>();

        Task<ImmutableArray<TResult>> IOpenIddictApplicationStore<IOpenIdApplication>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientIdAsync(IOpenIdApplication application,
            string identifier, CancellationToken cancellationToken)
            => SetClientIdAsync((TApplication) application, identifier, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientSecretAsync(IOpenIdApplication application, string secret, CancellationToken cancellationToken)
            => SetClientSecretAsync((TApplication) application, secret, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientTypeAsync(IOpenIdApplication application, string type, CancellationToken cancellationToken)
            => SetClientTypeAsync((TApplication) application, type, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetConsentTypeAsync(IOpenIdApplication application, string type, CancellationToken cancellationToken)
            => SetConsentTypeAsync((TApplication) application, type, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetDisplayNameAsync(IOpenIdApplication application, string name, CancellationToken cancellationToken)
            => SetDisplayNameAsync((TApplication) application, name, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetPermissionsAsync(IOpenIdApplication application, ImmutableArray<string> permissions, CancellationToken cancellationToken)
            => SetPermissionsAsync((TApplication) application, permissions, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetPostLogoutRedirectUrisAsync(IOpenIdApplication application,
            ImmutableArray<string> addresses, CancellationToken cancellationToken)
            => SetPostLogoutRedirectUrisAsync((TApplication) application, addresses, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetPropertiesAsync(IOpenIdApplication application, JObject properties, CancellationToken cancellationToken)
            => SetPropertiesAsync((TApplication) application, properties, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetRedirectUrisAsync(IOpenIdApplication application,
            ImmutableArray<string> addresses, CancellationToken cancellationToken)
            => SetRedirectUrisAsync((TApplication) application, addresses, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.UpdateAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => UpdateAsync((TApplication) application, cancellationToken);

        // ---------------------------------------------------------
        // Methods defined by the IOpenIdApplicationStore interface:
        // ---------------------------------------------------------

        async Task<IOpenIdApplication> IOpenIdApplicationStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdApplicationStore.GetPhysicalIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TApplication) application, cancellationToken);

        Task<ImmutableArray<string>> IOpenIdApplicationStore.GetRolesAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetRolesAsync((TApplication) application, cancellationToken);

        async Task<ImmutableArray<IOpenIdApplication>> IOpenIdApplicationStore.ListInRoleAsync(string role, CancellationToken cancellationToken)
            => (await ListInRoleAsync(role, cancellationToken)).CastArray<IOpenIdApplication>();

        Task IOpenIdApplicationStore.SetRolesAsync(IOpenIdApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken)
            => SetRolesAsync((TApplication) application, roles, cancellationToken);
    }
}
