using System;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using YesSql;

namespace OrchardCore.OpenId.YesSql.Services
{
    public class OpenIdScopeStore : IOpenIdScopeStore
    {
        private readonly ISession _session;

        public OpenIdScopeStore(ISession session)
        {
            _session = session;
        }

        /// <summary>
        /// Determines the number of scopes that exist in the database.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of scopes in the database.
        /// </returns>
        public virtual async Task<long> CountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<OpenIdScope>().CountAsync();
        }

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
        public virtual Task<long> CountAsync<TResult>(Func<IQueryable<OpenIdScope>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Creates a new scope.
        /// </summary>
        /// <param name="scope">The scope to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task CreateAsync(OpenIdScope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(scope);
            return _session.CommitAsync();
        }

        /// <summary>
        /// Removes an existing scope.
        /// </summary>
        /// <param name="scope">The scope to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task DeleteAsync(OpenIdScope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Delete(scope);

            return _session.CommitAsync();
        }

        /// <summary>
        /// Retrieves an authorization using its unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        public virtual Task<OpenIdScope> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<OpenIdScope, OpenIdScopeIndex>(index => index.ScopeId == identifier).FirstOrDefaultAsync();
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
        public virtual Task<OpenIdScope> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.GetAsync<OpenIdScope>(int.Parse(identifier, CultureInfo.InvariantCulture));
        }

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
        public virtual Task<TResult> GetAsync<TState, TResult>(
            Func<IQueryable<OpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Retrieves the description associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the description associated with the specified scope.
        /// </returns>
        public virtual Task<string> GetDescriptionAsync(OpenIdScope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return Task.FromResult(scope.Description);
        }

        /// <summary>
        /// Retrieves the unique identifier associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the unique identifier associated with the scope.
        /// </returns>
        public virtual Task<string> GetIdAsync(OpenIdScope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return Task.FromResult(scope.ScopeId);
        }

        /// <summary>
        /// Retrieves the name associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the name associated with the specified scope.
        /// </returns>
        public virtual Task<string> GetNameAsync(OpenIdScope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return Task.FromResult(scope.Name);
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
        public virtual Task<string> GetPhysicalIdAsync(OpenIdScope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return Task.FromResult(scope.Id.ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Retrieves the additional properties associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose
        /// result returns all the additional properties associated with the scope.
        /// </returns>
        public virtual Task<JObject> GetPropertiesAsync(OpenIdScope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return Task.FromResult(scope.Properties ?? new JObject());
        }

        /// <summary>
        /// Instantiates a new scope.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the instantiated scope, that can be persisted in the database.
        /// </returns>
        public virtual Task<OpenIdScope> InstantiateAsync(CancellationToken cancellationToken)
            => Task.FromResult(new OpenIdScope { ScopeId = Guid.NewGuid().ToString("n") });

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
        public virtual async Task<ImmutableArray<OpenIdScope>> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = _session.Query<OpenIdScope>();

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            return ImmutableArray.CreateRange(await query.ListAsync());
        }

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
        public virtual Task<ImmutableArray<TResult>> ListAsync<TState, TResult>(
            Func<IQueryable<OpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <summary>
        /// Sets the description associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="description">The description associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetDescriptionAsync(OpenIdScope scope, string description, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            scope.Description = description;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the name associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="name">The name associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetNameAsync(OpenIdScope scope, string name, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            scope.Name = name;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Sets the additional properties associated with a scope.
        /// </summary>
        /// <param name="scope">The scope.</param>
        /// <param name="properties">The additional properties associated with the scope </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task SetPropertiesAsync(OpenIdScope scope, JObject properties, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            scope.Properties = properties;

            return Task.CompletedTask;
        }

        /// <summary>
        /// Updates an existing scope.
        /// </summary>
        /// <param name="scope">The scope to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public virtual Task UpdateAsync(OpenIdScope scope, CancellationToken cancellationToken)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(scope);

            return _session.CommitAsync();
        }

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
            => CreateAsync((OpenIdScope) scope, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.DeleteAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdScope) scope, cancellationToken);

        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        Task<TResult> IOpenIddictScopeStore<IOpenIdScope>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetDescriptionAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetDescriptionAsync((OpenIdScope) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetIdAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdScope) scope, cancellationToken);

        Task<string> IOpenIddictScopeStore<IOpenIdScope>.GetNameAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetNameAsync((OpenIdScope) scope, cancellationToken);

        Task<JObject> IOpenIddictScopeStore<IOpenIdScope>.GetPropertiesAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetPropertiesAsync((OpenIdScope) scope, cancellationToken);

        async Task<IOpenIdScope> IOpenIddictScopeStore<IOpenIdScope>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdScope>> IOpenIddictScopeStore<IOpenIdScope>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdScope>();

        Task<ImmutableArray<TResult>> IOpenIddictScopeStore<IOpenIdScope>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdScope>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetDescriptionAsync(IOpenIdScope scope, string description, CancellationToken cancellationToken)
            => SetDescriptionAsync((OpenIdScope) scope, description, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetNameAsync(IOpenIdScope scope, string name, CancellationToken cancellationToken)
            => SetNameAsync((OpenIdScope) scope, name, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.SetPropertiesAsync(IOpenIdScope scope, JObject properties, CancellationToken cancellationToken)
            => SetPropertiesAsync((OpenIdScope) scope, properties, cancellationToken);

        Task IOpenIddictScopeStore<IOpenIdScope>.UpdateAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdScope) scope, cancellationToken);

        // ---------------------------------------------------
        // Methods defined by the IOpenIdScopeStore interface:
        // ---------------------------------------------------

        async Task<IOpenIdScope> IOpenIdScopeStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdScopeStore.GetPhysicalIdAsync(IOpenIdScope scope, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((OpenIdScope) scope, cancellationToken);
    }
}
