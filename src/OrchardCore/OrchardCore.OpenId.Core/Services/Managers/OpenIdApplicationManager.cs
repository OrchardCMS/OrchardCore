using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdApplicationManager : OpenIddictApplicationManager<IOpenIdApplication>
    {
        public OpenIdApplicationManager(
            IOpenIdApplicationStore store,
            ILogger<OpenIdApplicationManager> logger)
            : base(store, logger)
        {
        }

        protected new IOpenIdApplicationStore Store => (IOpenIdApplicationStore) base.Store;

        /// <summary>
        /// Retrieves an application using its physical identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client application corresponding to the identifier.
        /// </returns>
        public virtual Task<IOpenIdApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return Store.FindByPhysicalIdAsync(identifier, cancellationToken);
        }

        /// <summary>
        /// Retrieves the physical identifier associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the application.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(IOpenIdApplication application, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Store.GetPhysicalIdAsync(application, cancellationToken);
        }

        public virtual async Task AddToRoleAsync(IOpenIdApplication application,
            string role, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var roles = await Store.GetRolesAsync(application, cancellationToken);
            await Store.SetRolesAsync(application, roles.Add(role), cancellationToken);
            await UpdateAsync(application, cancellationToken);
        }

        public virtual Task<ImmutableArray<string>> GetRolesAsync(
            IOpenIdApplication application, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Store.GetRolesAsync(application, cancellationToken);
        }

        public virtual async Task<bool> IsInRoleAsync(IOpenIdApplication application,
            string role, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            return (await Store.GetRolesAsync(application, cancellationToken)).Contains(role, StringComparer.OrdinalIgnoreCase);
        }

        public virtual Task<ImmutableArray<IOpenIdApplication>> ListInRoleAsync(
            string role, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            return Store.ListInRoleAsync(role, cancellationToken);
        }

        public virtual async Task RemoveFromRoleAsync(IOpenIdApplication application,
            string role, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            var roles = await Store.GetRolesAsync(application, cancellationToken);
            await Store.SetRolesAsync(application, roles.Remove(role, StringComparer.OrdinalIgnoreCase), cancellationToken);
            await UpdateAsync(application, cancellationToken);
        }

        // TODO: remove when OpenIddict exposes this method.
        public virtual async Task UpdateAsync(IOpenIdApplication application,
            OpenIdApplicationDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            // Store the original client secret for later comparison.
            var secret = await Store.GetClientSecretAsync(application, cancellationToken);
            await PopulateAsync(application, descriptor, cancellationToken);

            // If the client secret was updated, re-obfuscate it before persisting the changes.
            var comparand = await Store.GetClientSecretAsync(application, cancellationToken);
            if (!string.Equals(secret, comparand, StringComparison.Ordinal))
            {
                await UpdateAsync(application, comparand, cancellationToken);

                return;
            }

            await UpdateAsync(application, cancellationToken);
        }

        protected override async Task PopulateAsync(IOpenIdApplication application,
            OpenIddictApplicationDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            await base.PopulateAsync(application, descriptor, cancellationToken);

            if (descriptor is OpenIdApplicationDescriptor instance)
            {
                await Store.SetRolesAsync(application, instance.Roles.ToImmutableArray(), cancellationToken);
            }
        }

        public async Task PopulateAsync(OpenIddictApplicationDescriptor descriptor,
            IOpenIdApplication application, CancellationToken cancellationToken = default)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            // TODO: remove when OpenIddict exposes this method.
            descriptor.ClientId = await Store.GetClientIdAsync(application, cancellationToken);
            descriptor.ClientSecret = await Store.GetClientSecretAsync(application, cancellationToken);
            descriptor.ConsentType = await Store.GetConsentTypeAsync(application, cancellationToken);
            descriptor.DisplayName = await Store.GetDisplayNameAsync(application, cancellationToken);
            descriptor.Type = await Store.GetClientTypeAsync(application, cancellationToken);
            descriptor.Permissions.UnionWith(await Store.GetPermissionsAsync(application, cancellationToken));

            foreach (var address in await Store.GetPostLogoutRedirectUrisAsync(application, cancellationToken))
            {
                // Ensure the address is not null or empty.
                if (string.IsNullOrEmpty(address))
                {
                    throw new ArgumentException("Callback URLs cannot be null or empty.");
                }

                // Ensure the address is a valid absolute URL.
                if (!Uri.TryCreate(address, UriKind.Absolute, out Uri uri) || !uri.IsWellFormedOriginalString())
                {
                    throw new ArgumentException("Callback URLs must be valid absolute URLs.");
                }

                descriptor.PostLogoutRedirectUris.Add(uri);
            }

            foreach (var address in await Store.GetRedirectUrisAsync(application, cancellationToken))
            {
                // Ensure the address is not null or empty.
                if (string.IsNullOrEmpty(address))
                {
                    throw new ArgumentException("Callback URLs cannot be null or empty.");
                }

                // Ensure the address is a valid absolute URL.
                if (!Uri.TryCreate(address, UriKind.Absolute, out Uri uri) || !uri.IsWellFormedOriginalString())
                {
                    throw new ArgumentException("Callback URLs must be valid absolute URLs.");
                }

                descriptor.RedirectUris.Add(uri);
            }

            if (descriptor is OpenIdApplicationDescriptor instance)
            {
                instance.Roles.UnionWith(await Store.GetRolesAsync(application, cancellationToken));
            }
        }
    }
}
