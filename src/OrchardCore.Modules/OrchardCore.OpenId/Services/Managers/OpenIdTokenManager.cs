using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdTokenManager : OpenIddictTokenManager<IOpenIdToken>
    {
        public OpenIdTokenManager(
            IOpenIdTokenStore store,
            ILogger<OpenIdTokenManager> logger)
            : base(store, logger)
        {
        }

        protected new IOpenIdTokenStore Store => (IOpenIdTokenStore) base.Store;

        /// <summary>
        /// Retrieves a token using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the token corresponding to the physical identifier.
        /// </returns>
        public virtual Task<IOpenIdToken> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return Store.FindByPhysicalIdAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Retrieves the physical identifier associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the token.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(IOpenIdToken token, CancellationToken cancellationToken = default)
        {
            if (token == null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            return Store.GetPhysicalIdAsync(token, cancellationToken);
        }
    }
}
