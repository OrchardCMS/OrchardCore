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
            IOpenIddictScopeCache<TScope> cache,
            ILogger<OpenIddictScopeManager<TScope>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options,
            IOpenIddictScopeStoreResolver resolver)
            : base(cache, logger, options, resolver)
        {
        }

        /// <summary>
        /// Retrieves a scope using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the scope corresponding to the identifier.
        /// </returns>
        public virtual ValueTask<TScope> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return Store is IOpenIdScopeStore<TScope> store ?
                store.FindByPhysicalIdAsync(identifier, cancellationToken) :
                Store.FindByIdAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Retrieves the physical identifier associated with an authorization.
        /// </summary>
        /// <param name="scope">The scope.</param>
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

            return Store is IOpenIdScopeStore<TScope> store ?
                store.GetPhysicalIdAsync(scope, cancellationToken) :
                Store.GetIdAsync(scope, cancellationToken);
        }

        async ValueTask<object> IOpenIdScopeManager.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        ValueTask<string> IOpenIdScopeManager.GetPhysicalIdAsync(object scope, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TScope)scope, cancellationToken);
    }
}
