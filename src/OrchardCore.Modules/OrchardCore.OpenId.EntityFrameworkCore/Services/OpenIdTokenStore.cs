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
    public class OpenIdTokenStore<TContext, TKey> : OpenIdTokenStore<OpenIdToken<TKey>,
                                                                     OpenIdApplication<TKey>,
                                                                     OpenIdAuthorization<TKey>, TContext, TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdTokenStore(TContext context)
            : base(context)
        {
        }
    }

    public class OpenIdTokenStore<TToken, TApplication, TAuthorization, TContext, TKey> :
        OpenIddictTokenStore<TToken, TApplication, TAuthorization, TContext, TKey>, IOpenIdTokenStore
        where TToken : OpenIdToken<TKey, TApplication, TAuthorization>, new()
        where TApplication : OpenIdApplication<TKey, TAuthorization, TToken>, new()
        where TAuthorization : OpenIdAuthorization<TKey, TApplication, TToken>, new()
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
        public virtual Task<TToken> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
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
        public virtual Task<string> GetPhysicalIdAsync(TToken token, CancellationToken cancellationToken)
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

        Task IOpenIddictTokenStore<IOpenIdToken>.CreateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => CreateAsync((TToken) token, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.DeleteAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => DeleteAsync((TToken) token, cancellationToken);

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
            => GetApplicationIdAsync((TToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetAuthorizationIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetAuthorizationIdAsync((TToken) token, cancellationToken);

        Task<DateTimeOffset?> IOpenIddictTokenStore<IOpenIdToken>.GetCreationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetCreationDateAsync((TToken) token, cancellationToken);

        Task<DateTimeOffset?> IOpenIddictTokenStore<IOpenIdToken>.GetExpirationDateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetExpirationDateAsync((TToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetIdAsync((TToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetPayloadAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPayloadAsync((TToken) token, cancellationToken);

        Task<JObject> IOpenIddictTokenStore<IOpenIdToken>.GetPropertiesAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPropertiesAsync((TToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetReferenceIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetReferenceIdAsync((TToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetStatusAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetStatusAsync((TToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetSubjectAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetSubjectAsync((TToken) token, cancellationToken);

        Task<string> IOpenIddictTokenStore<IOpenIdToken>.GetTokenTypeAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetTokenTypeAsync((TToken) token, cancellationToken);

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
            => SetApplicationIdAsync((TToken) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetAuthorizationIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetAuthorizationIdAsync((TToken) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetCreationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetCreationDateAsync((TToken) token, date, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetExpirationDateAsync(IOpenIdToken token, DateTimeOffset? date, CancellationToken cancellationToken)
            => SetExpirationDateAsync((TToken) token, date, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetPayloadAsync(IOpenIdToken token, string payload, CancellationToken cancellationToken)
            => SetPayloadAsync((TToken) token, payload, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetPropertiesAsync(IOpenIdToken token, JObject properties, CancellationToken cancellationToken)
            => SetPropertiesAsync((TToken) token, properties, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetReferenceIdAsync(IOpenIdToken token, string identifier, CancellationToken cancellationToken)
            => SetReferenceIdAsync((TToken) token, identifier, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetStatusAsync(IOpenIdToken token, string status, CancellationToken cancellationToken)
            => SetStatusAsync((TToken) token, status, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetSubjectAsync(IOpenIdToken token, string subject, CancellationToken cancellationToken)
            => SetSubjectAsync((TToken) token, subject, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.SetTokenTypeAsync(IOpenIdToken token, string type, CancellationToken cancellationToken)
            => SetTokenTypeAsync((TToken) token, type, cancellationToken);

        Task IOpenIddictTokenStore<IOpenIdToken>.UpdateAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => UpdateAsync((TToken) token, cancellationToken);

        // ---------------------------------------------------
        // Methods defined by the IOpenIdTokenStore interface:
        // ---------------------------------------------------

        async Task<IOpenIdToken> IOpenIdTokenStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdTokenStore.GetPhysicalIdAsync(IOpenIdToken token, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TToken) token, cancellationToken);
    }
}
