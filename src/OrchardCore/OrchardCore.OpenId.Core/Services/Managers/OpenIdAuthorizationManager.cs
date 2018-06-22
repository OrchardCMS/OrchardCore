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
    public class OpenIdAuthorizationManager<TAuthorization> : OpenIddictAuthorizationManager<TAuthorization>,
        IOpenIdAuthorizationManager where TAuthorization : class
    {
        public OpenIdAuthorizationManager(
            IOpenIddictAuthorizationStoreResolver resolver,
            ILogger<OpenIddictAuthorizationManager<TAuthorization>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options)
            : base(resolver, logger, options)
        {
        }

        protected new IOpenIdAuthorizationStore<TAuthorization> Store => (IOpenIdAuthorizationStore<TAuthorization>) base.Store;

        /// <summary>
        /// Retrieves an authorization using its physical identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        public virtual Task<TAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
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
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the authorization.
        /// </returns>
        public virtual ValueTask<string> GetPhysicalIdAsync(TAuthorization authorization, CancellationToken cancellationToken = default)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return Store.GetPhysicalIdAsync(authorization, cancellationToken);
        }

        async Task<object> IOpenIdAuthorizationManager.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        ValueTask<string> IOpenIdAuthorizationManager.GetPhysicalIdAsync(object authorization, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TAuthorization) authorization, cancellationToken);
    }
}
