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
    public class OpenIdAuthorizationStore<TContext, TKey> : OpenIdAuthorizationStore<OpenIdAuthorization<TKey>,
                                                                                     OpenIdApplication<TKey>,
                                                                                     OpenIdToken<TKey>, TContext, TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdAuthorizationStore(TContext context, IMemoryCache cache)
            : base(context, cache)
        {
        }
    }

    public class OpenIdAuthorizationStore<TAuthorization, TApplication, TToken, TContext, TKey> :
        OpenIddictAuthorizationStore<TAuthorization, TApplication, TToken, TContext, TKey>, IOpenIdAuthorizationStore
        where TAuthorization : OpenIdAuthorization<TKey, TApplication, TToken>, new()
        where TApplication : OpenIdApplication<TKey, TAuthorization, TToken>, new()
        where TToken : OpenIdToken<TKey, TApplication, TAuthorization>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdAuthorizationStore(TContext context, IMemoryCache cache)
            : base(context, cache)
        {
        }

        /// <summary>
        /// Retrieves an authorization using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        public virtual Task<TAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base FindByIdAsync() method is called.
            => FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves the physical identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the authorization.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync(authorization, cancellationToken);

        // Note: the following methods are deliberately implemented as explicit methods so they are not
        // exposed by Intellisense. Their logic MUST be limited to dealing with casts and downcasts.
        // Developers who need to customize the logic SHOULD override the methods taking concretes types.

        // ---------------------------------------------------------------
        // Methods defined by the IOpenIddictAuthorizationStore interface:
        // ---------------------------------------------------------------

        Task<long> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        Task<long> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CountAsync<TResult>(Func<IQueryable<IOpenIdAuthorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.CreateAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => CreateAsync((TAuthorization) authorization, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.DeleteAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => DeleteAsync((TAuthorization) authorization, cancellationToken);

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindAsync(string subject, string client, CancellationToken cancellationToken)
            => (await FindAsync(subject, client, cancellationToken)).CastArray<IOpenIdAuthorization>();

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindAsync(string subject, string client, string status, CancellationToken cancellationToken)
            => (await FindAsync(subject, client, status, cancellationToken)).CastArray<IOpenIdAuthorization>();

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindAsync(
            string subject, string client, string status, string type, CancellationToken cancellationToken)
            => (await FindAsync(subject, client, status, type, cancellationToken)).CastArray<IOpenIdAuthorization>();

        async Task<IOpenIdAuthorization> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.FindBySubjectAsync(string subject, CancellationToken cancellationToken)
            => (await FindBySubjectAsync(subject, cancellationToken)).CastArray<IOpenIdAuthorization>();

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetApplicationIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetApplicationIdAsync((TAuthorization) authorization, cancellationToken);

        Task<TResult> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetIdAsync((TAuthorization) authorization, cancellationToken);

        Task<JObject> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetPropertiesAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetPropertiesAsync((TAuthorization) authorization, cancellationToken);

        Task<ImmutableArray<string>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetScopesAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetScopesAsync((TAuthorization) authorization, cancellationToken);

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetStatusAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetStatusAsync((TAuthorization) authorization, cancellationToken);

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetSubjectAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetSubjectAsync((TAuthorization) authorization, cancellationToken);

        Task<string> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.GetTypeAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetTypeAsync((TAuthorization) authorization, cancellationToken);

        async Task<IOpenIdAuthorization> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdAuthorization>();

        Task<ImmutableArray<TResult>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        async Task<ImmutableArray<IOpenIdAuthorization>> IOpenIddictAuthorizationStore<IOpenIdAuthorization>.ListInvalidAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListInvalidAsync(count, offset, cancellationToken)).CastArray<IOpenIdAuthorization>();

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetApplicationIdAsync(IOpenIdAuthorization authorization,
            string identifier, CancellationToken cancellationToken)
            => SetApplicationIdAsync((TAuthorization) authorization, identifier, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetPropertiesAsync(IOpenIdAuthorization authorization, JObject properties, CancellationToken cancellationToken)
            => SetPropertiesAsync((TAuthorization) authorization, properties, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetScopesAsync(IOpenIdAuthorization authorization,
            ImmutableArray<string> scopes, CancellationToken cancellationToken)
            => SetScopesAsync((TAuthorization) authorization, scopes, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetStatusAsync(IOpenIdAuthorization authorization,
            string status, CancellationToken cancellationToken)
            => SetStatusAsync((TAuthorization) authorization, status, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetSubjectAsync(IOpenIdAuthorization authorization,
            string subject, CancellationToken cancellationToken)
            => SetSubjectAsync((TAuthorization) authorization, subject, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.SetTypeAsync(IOpenIdAuthorization authorization,
            string type, CancellationToken cancellationToken)
            => SetTypeAsync((TAuthorization) authorization, type, cancellationToken);

        Task IOpenIddictAuthorizationStore<IOpenIdAuthorization>.UpdateAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => UpdateAsync((TAuthorization) authorization, cancellationToken);

        // -----------------------------------------------------------
        // Methods defined by the IOpenIdAuthorizationStore interface:
        // -----------------------------------------------------------

        async Task<IOpenIdAuthorization> IOpenIdAuthorizationStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<string> IOpenIdAuthorizationStore.GetPhysicalIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TAuthorization) authorization, cancellationToken);
    }
}
