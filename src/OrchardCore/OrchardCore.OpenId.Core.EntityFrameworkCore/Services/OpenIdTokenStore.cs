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
    public class OpenIdTokenStore<TContext, TKey> : OpenIdTokenStore<OpenIddictToken<TKey>,
                                                                     OpenIddictApplication<TKey>,
                                                                     OpenIddictAuthorization<TKey>, TContext, TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdTokenStore(
            IMemoryCache cache,
            TContext context,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
            : base(cache, context, options)
        {
        }
    }

    public class OpenIdTokenStore<TToken, TApplication, TAuthorization, TContext, TKey> :
        OpenIddictTokenStore<TToken, TApplication, TAuthorization, TContext, TKey>, IOpenIdTokenStore<TToken>
        where TToken : OpenIddictToken<TKey, TApplication, TAuthorization>, new()
        where TApplication : OpenIddictApplication<TKey, TAuthorization, TToken>, new()
        where TAuthorization : OpenIddictAuthorization<TKey, TApplication, TToken>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdTokenStore(
            IMemoryCache cache,
            TContext context,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
            : base(cache, context, options)
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
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the token.
        /// </returns>
        public virtual ValueTask<string> GetPhysicalIdAsync(TToken token, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync(token, cancellationToken);
    }
}
