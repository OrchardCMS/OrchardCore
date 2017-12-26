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
        /// Determines the number of scopes that exist in the database.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of scopes in the database.
        /// </returns>
        Task<long> IOpenIddictScopeStore<IOpenIdScope>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        /// <summary>
        /// Determines the number of scopes that match the specified query.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of scopes that match the specified query.
        /// </returns>
        Task<long> IOpenIddictScopeStore<IOpenIdScope>.CountAsync<TResult>(Func<IQueryable<IOpenIdScope>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        /// <summary>
        /// Creates a new scope.
        /// </summary>
        /// <param name="scope">The scope to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result returns the scope.
        /// </returns>
        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.CreateAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => await CreateAsync((OpenIdScope<TKey>) scope, cancellationToken);

        /// <summary>
        /// Removes an existing scope.
        /// </summary>
        /// <param name="scope">The scope to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictScopeStore<IOpenIdScope>.DeleteAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdScope<TKey>) scope, cancellationToken);

        /// <summary>
        /// Executes the specified query and returns the first element.
        /// </summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="state">The optional state.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the first element returned when executing the query.
        /// </returns>
        Task<TResult> IOpenIddictScopeStore<IOpenIdScope>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        /// <summary>
        /// Retrieves the description associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the description associated with the specified scope.
        /// </returns>
        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetDescriptionAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetDescriptionAsync((OpenIdScope<TKey>) scope, cancellationToken);

        /// <summary>
        /// Retrieves the name associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the name associated with the specified scope.
        /// </returns>
        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetNameAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetNameAsync((OpenIdScope<TKey>) scope, cancellationToken);

        /// <summary>
        /// Instantiates a new scope.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the instantiated scope, that can be persisted in the database.
        /// </returns>
        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        /// <summary>
        /// Executes the specified query and returns all the corresponding elements.
        /// </summary>
        /// <param name="count">The number of results to return.</param>
        /// <param name="offset">The number of results to skip.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        async Task<ImmutableArray<IOpenIdScope>> IOpenIddictScopeStore<IOpenIdScope>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdScope>();

        /// <summary>
        /// Executes the specified query and returns all the corresponding elements.
        /// </summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="state">The optional state.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        Task<ImmutableArray<TResult>> IOpenIddictScopeStore<IOpenIdScope>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        /// <summary>
        /// Sets the description associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="description">The description associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictScopeStore<IOpenIdScope>.SetDescriptionAsync(IOpenIdScope scope, string description, CancellationToken cancellationToken)
            => SetDescriptionAsync((OpenIdScope<TKey>) scope, description, cancellationToken);

        /// <summary>
        /// Sets the name associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="name">The name associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictScopeStore<IOpenIdScope>.SetNameAsync(IOpenIdScope scope, string name, CancellationToken cancellationToken)
            => SetNameAsync((OpenIdScope<TKey>) scope, name, cancellationToken);

        /// <summary>
        /// Updates an existing scope.
        /// </summary>
        /// <param name="scope">The scope to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictScopeStore<IOpenIdScope>.UpdateAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdScope<TKey>) scope, cancellationToken);
    }
}
