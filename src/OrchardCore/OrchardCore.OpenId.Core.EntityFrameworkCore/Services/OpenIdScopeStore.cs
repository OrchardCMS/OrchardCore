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

namespace OrchardCore.OpenId.EntityFrameworkCore.Services
{
    public class OpenIdScopeStore<TContext, TKey> : OpenIdScopeStore<OpenIdScope<TKey>, TContext, TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdScopeStore(TContext context, IMemoryCache cache)
            : base(context, cache)
        {
        }
    }

    public class OpenIdScopeStore<TScope, TContext, TKey> : OpenIddictScopeStore<TScope, TContext, TKey>, IOpenIdScopeStore
        where TScope : OpenIdScope<TKey>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdScopeStore(TContext context, IMemoryCache cache)
            : base(context, cache)
        {
        }

        /// <summary>
        /// Retrieves a scope using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the scope corresponding to the identifier.
        /// </returns>
        public virtual Task<TScope> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base FindByIdAsync() method is called.
            => FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves all the scopes that contain the specified resource.
        /// </summary>
        /// <param name="resource">The resource associated with the scopes.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the scopes associated with the specified resource.
        /// </returns>
        public virtual async Task<ImmutableArray<TScope>> FindByResourceAsync(string resource, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentException("The resource cannot be null or empty.", nameof(resource));
            }

            // To optimize the efficiency of the query a bit, only scopes whose stringified
            // Resources column contains the specified resource are returned. Once the applications
            // are retrieved, a second pass is made to ensure only valid elements are returned.
            // Implementers that use this method in a hot path may want to override this method
            // to use SQL Server 2016 functions like JSON_VALUE to make the query more efficient.
            IQueryable<TScope> Query(IQueryable<TScope> scopes, string state)
                => from scope in scopes
                   where scope.Resources.Contains(state)
                   select scope;

            var builder = ImmutableArray.CreateBuilder<TScope>();

            foreach (var application in await ListAsync((applications, state) => Query(applications, state), resource, cancellationToken))
            {
                var resources = await GetResourcesAsync(application, cancellationToken);
                if (resources.Contains(resource, StringComparer.OrdinalIgnoreCase))
                {
                    builder.Add(application);
                }
            }

            return builder.ToImmutable();
        }

        /// <summary>
        /// Retrieves the physical identifier associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the scope.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(TScope scope, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync(scope, cancellationToken);

        // Note: the following methods are deliberately implemented as explicit methods so they are not
        // exposed by Intellisense. Their logic MUST be limited to dealing with casts and downcasts.
        // Developers who need to customize the logic SHOULD override the methods taking concretes types.

        // -------------------------------------------------------
        // Methods defined by the IOpenIddictScopeStore interface:
        // -------------------------------------------------------

        Task<long> IOpenIddictScopeStore<IOpenIdScope>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        Task<long> IOpenIddictScopeStore<IOpenIdScope>.CountAsync<TResult>(Func<IQueryable<IOpenIdScope>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.CreateAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => CreateAsync((TScope) scope, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.DeleteAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => DeleteAsync((TScope) scope, cancellationToken);

        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.FindByNameAsync(string name, CancellationToken cancellationToken)
            => await FindByNameAsync(name, cancellationToken);

        async Task<ImmutableArray<IOpenIdScope>> IOpenIddictScopeStore<IOpenIdScope>.FindByNamesAsync(
            ImmutableArray<string> names, CancellationToken cancellationToken)
            => (await FindByNamesAsync(names, cancellationToken)).CastArray<IOpenIdScope>();

        Task<TResult> IOpenIddictScopeStore<IOpenIdScope>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetDescriptionAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetDescriptionAsync((TScope) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetDisplayNameAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetDisplayNameAsync((TScope) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetIdAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetIdAsync((TScope) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetNameAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetNameAsync((TScope) scope, cancellationToken);

        Task<JObject> IOpenIddictScopeStore<IOpenIdScope>.GetPropertiesAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetPropertiesAsync((TScope) scope, cancellationToken);

        Task<ImmutableArray<string>> IOpenIddictScopeStore<IOpenIdScope>.GetResourcesAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetResourcesAsync((TScope) scope, cancellationToken);

        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdScope>> IOpenIddictScopeStore<IOpenIdScope>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdScope>();

        Task<ImmutableArray<TResult>> IOpenIddictScopeStore<IOpenIdScope>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetDescriptionAsync(IOpenIdScope scope, string description, CancellationToken cancellationToken)
            => SetDescriptionAsync((TScope) scope, description, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetDisplayNameAsync(IOpenIdScope scope, string name, CancellationToken cancellationToken)
            => SetDisplayNameAsync((TScope) scope, name, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetNameAsync(IOpenIdScope scope, string name, CancellationToken cancellationToken)
            => SetNameAsync((TScope) scope, name, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetPropertiesAsync(IOpenIdScope scope, JObject properties, CancellationToken cancellationToken)
            => SetPropertiesAsync((TScope) scope, properties, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetResourcesAsync(IOpenIdScope scope, ImmutableArray<string> resources, CancellationToken cancellationToken)
            => SetResourcesAsync((TScope) scope, resources, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.UpdateAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => UpdateAsync((TScope) scope, cancellationToken);

        // ---------------------------------------------------
        // Methods defined by the IOpenIdScopeStore interface:
        // ---------------------------------------------------

        async Task<IOpenIdScope> IOpenIdScopeStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        async Task<ImmutableArray<IOpenIdScope>> IOpenIdScopeStore.FindByResourceAsync(string resource, CancellationToken cancellationToken)
            => (await FindByResourceAsync(resource, cancellationToken)).CastArray<IOpenIdScope>();

        Task<string> IOpenIdScopeStore.GetPhysicalIdAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TScope) scope, cancellationToken);
    }
}
