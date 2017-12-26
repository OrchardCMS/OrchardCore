using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore;
using OrchardCore.OpenId.Abstractions;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.EntityFrameworkCore.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Services
{
    public class OpenIdAuthorizationStore<TContext, TKey> :
        OpenIddictAuthorizationStore<OpenIdAuthorization<TKey>, OpenIdApplication<TKey>, OpenIdToken<TKey>, TContext, TKey>,
        IOpenIdAuthorizationStore
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdAuthorizationStore(TContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Determines the number of authorizations that exist in the database.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of authorizations in the database.
        /// </returns>
        Task<long> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        /// <summary>
        /// Determines the number of authorizations that match the specified query.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of authorizations that match the specified query.
        /// </returns>
        Task<long> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CountAsync<TResult>(Func<IQueryable<IOpenIdAuthorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        /// <summary>
        /// Creates a new authorization.
        /// </summary>
        /// <param name="authorization">The authorization to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result returns the authorization.
        /// </returns>
        async Task<IOpenIdAuthorization> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CreateAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => await CreateAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);

        /// <summary>
        /// Removes an existing authorization.
        /// </summary>
        /// <param name="authorization">The authorization to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.DeleteAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);

        /// <summary>
        /// Retrieves the authorizations corresponding to the specified
        /// subject and associated with the application identifier.
        /// </summary>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="client">The client associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorizations corresponding to the subject/client.
        /// </returns>
        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindAsync(string subject, string client, CancellationToken cancellationToken)
            => (await FindAsync(subject, client, cancellationToken)).CastArray<IOpenIdAuthorization>();

        /// <summary>
        /// Retrieves an authorization using its unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        async Task<IOpenIdAuthorization> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves the optional application identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the application identifier associated with the authorization.
        /// </returns>
        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetApplicationIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetApplicationIdAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);

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
        Task<TResult> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        /// <summary>
        /// Retrieves the unique identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the unique identifier associated with the authorization.
        /// </returns>
        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);

        /// <summary>
        /// Retrieves the scopes associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the scopes associated with the specified authorization.
        /// </returns>
        Task<ImmutableArray<string>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetScopesAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetScopesAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);

        /// <summary>
        /// Retrieves the status associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the status associated with the specified authorization.
        /// </returns>
        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetStatusAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetStatusAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);

        /// <summary>
        /// Retrieves the subject associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the subject associated with the specified authorization.
        /// </returns>
        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetSubjectAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetSubjectAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);

        /// <summary>
        /// Retrieves the type associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the type associated with the specified authorization.
        /// </returns>
        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetTypeAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetTypeAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);

        /// <summary>
        /// Instantiates a new authorization.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result
        /// returns the instantiated authorization, that can be persisted in the database.
        /// </returns>
        async Task<IOpenIdAuthorization> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.InstantiateAsync(CancellationToken cancellationToken)
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
        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdAuthorization>();

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
        Task<ImmutableArray<TResult>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        /// <summary>
        /// Lists the ad-hoc authorizations that are marked as invalid or have no
        /// valid token attached and that can be safely removed from the database.
        /// </summary>
        /// <param name="count">The number of results to return.</param>
        /// <param name="offset">The number of results to skip.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListInvalidAsync(count, offset, cancellationToken)).CastArray<IOpenIdAuthorization>();

        /// <summary>
        /// Sets the application identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="identifier">The unique identifier associated with the client application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetApplicationIdAsync(IOpenIdAuthorization authorization,
            string identifier, CancellationToken cancellationToken)
            => SetApplicationIdAsync((OpenIdAuthorization<TKey>) authorization, identifier, cancellationToken);

        /// <summary>
        /// Sets the scopes associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="scopes">The scopes associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetScopesAsync(IOpenIdAuthorization authorization,
            ImmutableArray<string> scopes, CancellationToken cancellationToken)
            => SetScopesAsync((OpenIdAuthorization<TKey>) authorization, scopes, cancellationToken);

        /// <summary>
        /// Sets the status associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="status">The status associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetStatusAsync(IOpenIdAuthorization authorization,
            string status, CancellationToken cancellationToken)
            => SetStatusAsync((OpenIdAuthorization<TKey>) authorization, status, cancellationToken);

        /// <summary>
        /// Sets the subject associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="subject">The subject associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetSubjectAsync(IOpenIdAuthorization authorization,
            string subject, CancellationToken cancellationToken)
            => SetSubjectAsync((OpenIdAuthorization<TKey>) authorization, subject, cancellationToken);

        /// <summary>
        /// Sets the type associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="type">The type associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetTypeAsync(IOpenIdAuthorization authorization,
            string type, CancellationToken cancellationToken)
            => SetTypeAsync((OpenIdAuthorization<TKey>) authorization, type, cancellationToken);

        /// <summary>
        /// Updates an existing authorization.
        /// </summary>
        /// <param name="authorization">The authorization to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.UpdateAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdAuthorization<TKey>) authorization, cancellationToken);
    }
}
