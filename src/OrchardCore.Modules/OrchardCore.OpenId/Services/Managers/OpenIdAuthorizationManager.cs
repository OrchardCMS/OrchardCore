using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdAuthorizationManager : OpenIddictAuthorizationManager<IOpenIdAuthorization>
    {
        public OpenIdAuthorizationManager(
            IOpenIdAuthorizationStore store,
            ILogger<OpenIdAuthorizationManager> logger)
            : base(store, logger)
        {
        }

        protected new IOpenIdAuthorizationStore Store => (IOpenIdAuthorizationStore) base.Store;

        /// <summary>
        /// Retrieves an authorization using its physical identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        public virtual Task<IOpenIdAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return Store.FindByPhysicalIdAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Retrieves the physical identifier associated with an authorization.
        /// </summary>
        /// <param name="authorization">The authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the authorization.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(IOpenIdAuthorization authorization, CancellationToken cancellationToken = default)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Store.GetPhysicalIdAsync(authorization, cancellationToken);
        }
    }
}
