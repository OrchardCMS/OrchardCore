using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        public OpenIdScopeStore(TContext context)
            : base(context)
        {
        }
    }

    public class OpenIdScopeStore<TScope, TContext, TKey> : OpenIddictScopeStore<TScope, TContext, TKey>, IOpenIdScopeStore
        where TScope : OpenIdScope<TKey>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdScopeStore(TContext context)
            : base(context)
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

        Task<TResult> IOpenIddictScopeStore<IOpenIdScope>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetDescriptionAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetDescriptionAsync((TScope) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetIdAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetIdAsync((TScope) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetNameAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetNameAsync((TScope) scope, cancellationToken);

        Task<JObject> IOpenIddictScopeStore<IOpenIdScope>.GetPropertiesAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetPropertiesAsync((TScope) scope, cancellationToken);

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

        Task IOpenIddictScopeStore<IOpenIdScope>.SetNameAsync(IOpenIdScope scope, string name, CancellationToken cancellationToken)
            => SetNameAsync((TScope) scope, name, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetPropertiesAsync(IOpenIdScope scope, JObject properties, CancellationToken cancellationToken)
            => SetPropertiesAsync((TScope) scope, properties, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.UpdateAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => UpdateAsync((TScope) scope, cancellationToken);

        // ---------------------------------------------------
        // Methods defined by the IOpenIdScopeStore interface:
        // ---------------------------------------------------

        async Task<IOpenIdScope> IOpenIdScopeStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdScopeStore.GetPhysicalIdAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TScope) scope, cancellationToken);
    }
}
