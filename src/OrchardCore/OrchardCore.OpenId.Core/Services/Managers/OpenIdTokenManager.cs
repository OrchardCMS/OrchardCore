using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdTokenManager<TToken> : OpenIddictTokenManager<TToken>, IOpenIdTokenManager where TToken : class
    {
        public OpenIdTokenManager(
            IOpenIddictTokenCache<TToken> cache,
            ILogger<OpenIddictTokenManager<TToken>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options,
            IOpenIddictTokenStoreResolver resolver)
            : base(cache, logger, options, resolver)
        {
        }

        /// <summary>
        /// Retrieves a token using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token corresponding to the physical identifier.
        /// </returns>
        public virtual ValueTask<TToken> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return Store is IOpenIdTokenStore<TToken> store ?
                store.FindByPhysicalIdAsync(identifier, cancellationToken) :
                Store.FindByIdAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Retrieves the physical identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the token.
        /// </returns>
        public virtual ValueTask<string> GetPhysicalIdAsync(TToken token, CancellationToken cancellationToken = default)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Store is IOpenIdTokenStore<TToken> store ?
                store.GetPhysicalIdAsync(token, cancellationToken) :
                Store.GetIdAsync(token, cancellationToken);
        }

        async ValueTask<object> IOpenIdTokenManager.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        ValueTask<string> IOpenIdTokenManager.GetPhysicalIdAsync(object token, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TToken)token, cancellationToken);
    }
}
