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
    public class OpenIdTokenStore<TContext, TKey> :
        OpenIddictTokenStore<OpenIdToken<TKey>, OpenIdApplication<TKey>, OpenIdAuthorization<TKey>, TContext, TKey>, IOpenIdTokenStore
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdTokenStore(TContext context)
            : base(context)
        {
        }

        /// <summary>
        /// Determines the number of tokens that exist in the database.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of applications in the database.
        /// </returns>
        Task<long> IOpenIddictTokenStore<IOpenIdToken>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        /// <summary>
        /// Determines the number of tokens that match the specified query.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of tokens that match the specified query.
        /// </returns>
        Task<long> IOpenIddictTokenStore<IOpenIdToken>.CountAsync<TResult>(Func<IQueryable<IOpenIdToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        /// <summary>
        /// Creates a new token.
        /// </summary>
        /// <param name="token">The token to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result returns the token.
        /// </returns>
        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.CreateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => await CreateAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Removes a token.
        /// </summary>
        /// <param name="token">The token to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>A <see cref="Task"/> that can be used to monitor the asynchronous operation.</returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.DeleteAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the list of tokens corresponding to the specified application identifier.
        /// </summary>
        /// <param name="identifier">The application identifier associated with the tokens.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the tokens corresponding to the specified application.
        /// </returns>
        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
            => (await FindByApplicationIdAsync(identifier, cancellationToken)).CastArray<IOpenIdToken>();

        /// <summary>
        /// Retrieves the list of tokens corresponding to the specified authorization identifier.
        /// </summary>
        /// <param name="identifier">The authorization identifier associated with the tokens.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the tokens corresponding to the specified authorization.
        /// </returns>
        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
            => (await FindByAuthorizationIdAsync(identifier, cancellationToken)).CastArray<IOpenIdToken>();

        /// <summary>
        /// Retrieves the list of tokens corresponding to the specified reference identifier.
        /// Note: the reference identifier may be hashed or encrypted for security reasons.
        /// </summary>
        /// <param name="identifier">The reference identifier associated with the tokens.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the tokens corresponding to the specified reference identifier.
        /// </returns>
        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByReferenceIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves a token using its unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token corresponding to the unique identifier.
        /// </returns>
        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves the list of tokens corresponding to the specified subject.
        /// </summary>
        /// <param name="subject">The subject associated with the tokens.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the tokens corresponding to the specified subject.
        /// </returns>
        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindBySubjectAsync(string subject, CancellationToken cancellationToken)
            => (await FindBySubjectAsync(subject, cancellationToken)).CastArray<IOpenIdToken>();

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
        Task<TResult> IOpenIddictTokenStore<IOpenIdToken>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        /// <summary>
        /// Retrieves the optional application identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the application identifier associated with the token.
        /// </returns>
        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetApplicationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetApplicationIdAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the optional authorization identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization identifier associated with the token.
        /// </returns>
        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetAuthorizationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetAuthorizationIdAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the creation date associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the creation date associated with the specified token.
        /// </returns>
        Task<DateTimeOffset?> IOpenIddictTokenStore<IOpenIdToken>.GetCreationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetCreationDateAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the expiration date associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the expiration date associated with the specified token.
        /// </returns>
        Task<DateTimeOffset?> IOpenIddictTokenStore<IOpenIdToken>.GetExpirationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetExpirationDateAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the unique identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the unique identifier associated with the token.
        /// </returns>
        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the payload associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the payload associated with the specified token.
        /// </returns>
        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetPayloadAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPayloadAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the reference identifier associated with a token.
        /// Note: depending on the manager used to create the token,
        /// the reference identifier may be hashed for security reasons.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the reference identifier associated with the specified token.
        /// </returns>
        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetReferenceIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetReferenceIdAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the status associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the status associated with the specified token.
        /// </returns>
        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetStatusAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetStatusAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the subject associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the subject associated with the specified token.
        /// </returns>
        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetSubjectAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetSubjectAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves the token type associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token type associated with the specified token.
        /// </returns>
        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetTokenTypeAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetTokenTypeAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Instantiates a new token.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the instantiated token, that can be persisted in the database.
        /// </returns>
        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.InstantiateAsync(CancellationToken cancellationToken)
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
        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdToken>();

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
        Task<ImmutableArray<TResult>> IOpenIddictTokenStore<IOpenIdToken>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        /// <summary>
        /// Lists the tokens that are marked as expired or invalid
        /// and that can be safely removed from the database.
        /// </summary>
        /// <param name="count">The number of results to return.</param>
        /// <param name="offset">The number of results to skip.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListInvalidAsync(count, offset, cancellationToken)).CastArray<IOpenIdToken>();

        /// <summary>
        /// Sets the application identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="identifier">The unique identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetApplicationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetApplicationIdAsync((OpenIdToken<TKey>) token, identifier, cancellationToken);

        /// <summary>
        /// Sets the authorization identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="identifier">The unique identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetAuthorizationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetAuthorizationIdAsync((OpenIdToken<TKey>) token, identifier, cancellationToken);

        /// <summary>
        /// Sets the creation date associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="date">The creation date.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetCreationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetCreationDateAsync((OpenIdToken<TKey>) token, date, cancellationToken);

        /// <summary>
        /// Sets the expiration date associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="date">The expiration date.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetExpirationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetExpirationDateAsync((OpenIdToken<TKey>) token, date, cancellationToken);

        /// <summary>
        /// Sets the payload associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="payload">The payload associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetPayloadAsync(IOpenIdToken token, string payload, CancellationToken cancellationToken)
            => SetPayloadAsync((OpenIdToken<TKey>) token, payload, cancellationToken);

        /// <summary>
        /// Sets the reference identifier associated with a token.
        /// Note: depending on the manager used to create the token,
        /// the reference identifier may be hashed for security reasons.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="identifier">The reference identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetReferenceIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetReferenceIdAsync((OpenIdToken<TKey>) token, identifier, cancellationToken);

        /// <summary>
        /// Sets the status associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="status">The status associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetStatusAsync(IOpenIdToken token, string status, CancellationToken cancellationToken)
            => SetStatusAsync((OpenIdToken<TKey>) token, status, cancellationToken);

        /// <summary>
        /// Sets the subject associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="subject">The subject associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetSubjectAsync(IOpenIdToken token, string subject, CancellationToken cancellationToken)
            => SetSubjectAsync((OpenIdToken<TKey>) token, subject, cancellationToken);

        /// <summary>
        /// Sets the token type associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="type">The token type associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.SetTokenTypeAsync(IOpenIdToken token, string type, CancellationToken cancellationToken)
            => SetTokenTypeAsync((OpenIdToken<TKey>) token, type, cancellationToken);

        /// <summary>
        /// Updates an existing token.
        /// </summary>
        /// <param name="token">The token to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictTokenStore<IOpenIdToken>.UpdateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdToken<TKey>) token, cancellationToken);

        /// <summary>
        /// Retrieves a token using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token corresponding to the physical identifier.
        /// </returns>
        public virtual async Task<IOpenIdToken> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base FindByIdAsync() method is called.
            => await FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves the physical identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the token.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync((OpenIdToken<TKey>) token, cancellationToken);
    }
}
