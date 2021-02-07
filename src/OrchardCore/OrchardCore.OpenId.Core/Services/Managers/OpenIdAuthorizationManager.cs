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
            IOpenIddictAuthorizationCache<TAuthorization> cache,
            ILogger<OpenIddictAuthorizationManager<TAuthorization>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options,
            IOpenIddictAuthorizationStoreResolver resolver)
            : base(cache, logger, options, resolver)
        {
        }

        /// <summary>
        /// Retrieves an authorization using its physical identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the authorization.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the authorization corresponding to the identifier.
        /// </returns>
        public virtual ValueTask<TAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return Store is IOpenIdApplicationStore<TAuthorization> store ?
                store.FindByPhysicalIdAsync(identifier, cancellationToken) :
                Store.FindByIdAsync(identifier, cancellationToken);
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

            return Store is IOpenIdAuthorizationStore<TAuthorization> store ?
                store.GetPhysicalIdAsync(authorization, cancellationToken) :
                Store.GetIdAsync(authorization, cancellationToken);
        }

        async ValueTask<object> IOpenIdAuthorizationManager.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        ValueTask<string> IOpenIdAuthorizationManager.GetPhysicalIdAsync(object authorization, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TAuthorization)authorization, cancellationToken);
    }
}
