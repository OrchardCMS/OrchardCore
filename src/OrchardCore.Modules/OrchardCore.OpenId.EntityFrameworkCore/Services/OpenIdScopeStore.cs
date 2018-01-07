using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.EntityFrameworkCore.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Services
{
    public class OpenIdScopeStore<TContext, TKey> :
        OpenIddictScopeStore<OpenIdScope<TKey>, TContext, TKey>, IOpenIdScopeStore
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
        public virtual Task<OpenIdScope<TKey>> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
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
        public virtual Task<string> GetPhysicalIdAsync(OpenIdScope<TKey> scope, CancellationToken cancellationToken)
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

        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.CreateAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => await CreateAsync((OpenIdScope<TKey>) scope, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.DeleteAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdScope<TKey>) scope, cancellationToken);

        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        Task<TResult> IOpenIddictScopeStore<IOpenIdScope>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetDescriptionAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetDescriptionAsync((OpenIdScope<TKey>) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetIdAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdScope<TKey>) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetNameAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetNameAsync((OpenIdScope<TKey>) scope, cancellationToken);

        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdScope>> IOpenIddictScopeStore<IOpenIdScope>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdScope>();

        Task<ImmutableArray<TResult>> IOpenIddictScopeStore<IOpenIdScope>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetDescriptionAsync(IOpenIdScope scope, string description, CancellationToken cancellationToken)
            => SetDescriptionAsync((OpenIdScope<TKey>) scope, description, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetNameAsync(IOpenIdScope scope, string name, CancellationToken cancellationToken)
            => SetNameAsync((OpenIdScope<TKey>) scope, name, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.UpdateAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdScope<TKey>) scope, cancellationToken);

        // ---------------------------------------------------
        // Methods defined by the IOpenIdScopeStore interface:
        // ---------------------------------------------------

        async Task<IOpenIdScope> IOpenIdScopeStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdScopeStore.GetPhysicalIdAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((OpenIdScope<TKey>) scope, cancellationToken);
    }
}
