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
        /// Retrieves a token using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token corresponding to the physical identifier.
        /// </returns>
        public virtual Task<OpenIdToken<TKey>> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base FindByIdAsync() method is called.
            => FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves the physical identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the token.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(OpenIdToken<TKey> token, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync(token, cancellationToken);

        // Note: the following methods are deliberately implemented as explicit methods so they are not
        // exposed by Intellisense. Their logic MUST be limited to dealing with casts and downcasts.
        // Developers who need to customize the logic SHOULD override the methods taking concretes types.

        // -------------------------------------------------------
        // Methods defined by the IOpenIddictTokenStore interface:
        // -------------------------------------------------------

        Task<long> IOpenIddictTokenStore<IOpenIdToken>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        Task<long> IOpenIddictTokenStore<IOpenIdToken>.CountAsync<TResult>(Func<IQueryable<IOpenIdToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.CreateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => await CreateAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.DeleteAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdToken<TKey>) token, cancellationToken);

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
            => (await FindByApplicationIdAsync(identifier, cancellationToken)).CastArray<IOpenIdToken>();

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
            => (await FindByAuthorizationIdAsync(identifier, cancellationToken)).CastArray<IOpenIdToken>();

        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByReferenceIdAsync(identifier, cancellationToken);

        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.FindBySubjectAsync(string subject, CancellationToken cancellationToken)
            => (await FindBySubjectAsync(subject, cancellationToken)).CastArray<IOpenIdToken>();

        Task<TResult> IOpenIddictTokenStore<IOpenIdToken>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetApplicationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetApplicationIdAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetAuthorizationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetAuthorizationIdAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<DateTimeOffset?> IOpenIddictTokenStore<IOpenIdToken>.GetCreationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetCreationDateAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<DateTimeOffset?> IOpenIddictTokenStore<IOpenIdToken>.GetExpirationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetExpirationDateAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetPayloadAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPayloadAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetReferenceIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetReferenceIdAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetStatusAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetStatusAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetSubjectAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetSubjectAsync((OpenIdToken<TKey>) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetTokenTypeAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetTokenTypeAsync((OpenIdToken<TKey>) token, cancellationToken);

        async Task<IOpenIdToken> IOpenIddictTokenStore<IOpenIdToken>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdToken>();

        Task<ImmutableArray<TResult>> IOpenIddictTokenStore<IOpenIdToken>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdToken>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        async Task<ImmutableArray<IOpenIdToken>> IOpenIddictTokenStore<IOpenIdToken>.ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListInvalidAsync(count, offset, cancellationToken)).CastArray<IOpenIdToken>();

        Task IOpenIddictTokenStore<IOpenIdToken>.SetApplicationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetApplicationIdAsync((OpenIdToken<TKey>) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetAuthorizationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetAuthorizationIdAsync((OpenIdToken<TKey>) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetCreationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetCreationDateAsync((OpenIdToken<TKey>) token, date, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetExpirationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetExpirationDateAsync((OpenIdToken<TKey>) token, date, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetPayloadAsync(IOpenIdToken token, string payload, CancellationToken cancellationToken)
            => SetPayloadAsync((OpenIdToken<TKey>) token, payload, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetReferenceIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetReferenceIdAsync((OpenIdToken<TKey>) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetStatusAsync(IOpenIdToken token, string status, CancellationToken cancellationToken)
            => SetStatusAsync((OpenIdToken<TKey>) token, status, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetSubjectAsync(IOpenIdToken token, string subject, CancellationToken cancellationToken)
            => SetSubjectAsync((OpenIdToken<TKey>) token, subject, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetTokenTypeAsync(IOpenIdToken token, string type, CancellationToken cancellationToken)
            => SetTokenTypeAsync((OpenIdToken<TKey>) token, type, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.UpdateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdToken<TKey>) token, cancellationToken);

        // ---------------------------------------------------
        // Methods defined by the IOpenIdTokenStore interface:
        // ---------------------------------------------------

        async Task<IOpenIdToken> IOpenIdTokenStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdTokenStore.GetPhysicalIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((OpenIdToken<TKey>) token, cancellationToken);
    }
}
