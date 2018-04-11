using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdScopeManager : OpenIddictScopeManager<IOpenIdScope>
    {
        public OpenIdScopeManager(
            IOpenIdScopeStore store,
            ILogger<OpenIdScopeManager> logger)
            : base(store, logger)
        {
        }

        protected new IOpenIdScopeStore Store => (IOpenIdScopeStore) base.Store;

        /// <summary>
        /// Retrieves a scope using its physical identifier.
        /// </summary>
        /// <param name="identifier">The physical identifier associated with the scope.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the scope corresponding to the identifier.
        /// </returns>
        public virtual Task<IOpenIdScope> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return Store.FindByPhysicalIdAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Retrieves all the scopes that contain the specified resource.
        /// </summary>
        /// <param name="resource">The resource associated with the scopes.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the scopes associated with the specified resource.
        /// </returns>
        public virtual Task<ImmutableArray<IOpenIdScope>> FindByResourceAsync(string resource, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(resource))
            {
                throw new ArgumentException("The resource cannot be null or empty.", nameof(resource));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return Store.FindByResourceAsync(resource, cancellationToken);
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
        public virtual Task<string> GetPhysicalIdAsync(IOpenIdScope scope, CancellationToken cancellationToken = default)
        {
            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            return Store.GetPhysicalIdAsync(scope, cancellationToken);
        }

        public virtual async Task UpdateAsync(IOpenIdScope scope,
            OpenIddictScopeDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            await PopulateAsync(scope, descriptor, cancellationToken);
            await UpdateAsync(scope, cancellationToken);
        }

        public async Task PopulateAsync(OpenIddictScopeDescriptor descriptor,
            IOpenIdScope scope, CancellationToken cancellationToken = default)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (scope == null)
            {
                throw new ArgumentNullException(nameof(scope));
            }

            descriptor.Description = await Store.GetDescriptionAsync(scope, cancellationToken);
            descriptor.DisplayName = await Store.GetDisplayNameAsync(scope, cancellationToken);
            descriptor.Name = await Store.GetNameAsync(scope, cancellationToken);
            descriptor.Resources.UnionWith(await Store.GetResourcesAsync(scope, cancellationToken));
        }
    }
}
