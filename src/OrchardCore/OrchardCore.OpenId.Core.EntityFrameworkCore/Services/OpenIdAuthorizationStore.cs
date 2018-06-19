using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OpenIddict.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.EntityFrameworkCore.Services
{
    public class OpenIdAuthorizationStore<TContext, TKey> : OpenIdAuthorizationStore<OpenIddictAuthorization<TKey>,
                                                                                     OpenIddictApplication<TKey>,
                                                                                     OpenIddictToken<TKey>, TContext, TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdAuthorizationStore(
            IMemoryCache cache,
            TContext context,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
            : base(cache, context, options)
        {
        }
    }

    public class OpenIdAuthorizationStore<TAuthorization, TApplication, TToken, TContext, TKey> :
        OpenIddictAuthorizationStore<TAuthorization, TApplication, TToken, TContext, TKey>, IOpenIdAuthorizationStore<TAuthorization>
        where TAuthorization : OpenIddictAuthorization<TKey, TApplication, TToken>, new()
        where TApplication : OpenIddictApplication<TKey, TAuthorization, TToken>, new()
        where TToken : OpenIddictToken<TKey, TApplication, TAuthorization>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdAuthorizationStore(
            IMemoryCache cache,
            TContext context,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
            : base(cache, context, options)
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
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the authorization.
        /// </returns>
        public virtual ValueTask<string> GetPhysicalIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync(authorization, cancellationToken);
    }
}
