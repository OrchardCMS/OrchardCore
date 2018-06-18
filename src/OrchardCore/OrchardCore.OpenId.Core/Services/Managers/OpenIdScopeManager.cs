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
    public class OpenIdScopeManager<TScope> : OpenIddictScopeManager<TScope>, IOpenIdScopeManager where TScope : class
    {
        public OpenIdScopeManager(
            IOpenIddictScopeStoreResolver resolver,
            ILogger<OpenIddictScopeManager<TScope>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options)
            : base(resolver, logger, options)
        {
        }

        protected new IOpenIdScopeStore<TScope> Store => (IOpenIdScopeStore<TScope>) base.Store;

        /// <summary>
        /// Retrieves a scope using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the scope corresponding to the identifier.
        /// </returns>
        public virtual Task<TScope> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

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
        public virtual ValueTask<string> GetPhysicalIdAsync(TScope scope, CancellationToken cancellationToken = default)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return Store.GetPhysicalIdAsync(scope, cancellationToken);
        }

        async Task<object> IOpenIdScopeManager.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        ValueTask<string> IOpenIdScopeManager.GetPhysicalIdAsync(object scope, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TScope) scope, cancellationToken);
    }
}
