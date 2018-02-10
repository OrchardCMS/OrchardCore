using System;
using System.Collections.Immutable;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
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

        public virtual async Task<bool> IsConsentRequired(ClaimsPrincipal principal, string client,
            ImmutableArray<string> scopes, CancellationToken cancellationToken = default)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client cannot be null or empty.", nameof(client));
            }

            var subject = principal.FindFirstValue(OpenIdConnectConstants.Claims.Subject) ??
                          principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(subject))
            {
                throw new InvalidOperationException("The subject claim cannot be extracted from the principal.");
            }

            // Retrieve the authorizations associated with the logged in user
            // and the client application and that contain the requested scopes.
            // If at least one matching authorization can be found, return false
            // to indicate that explicit user consent is not required.
            foreach (var authorization in await FindAsync(subject, client, cancellationToken))
            {
                if (await IsValidAsync(authorization, cancellationToken) &&
                    await IsPermanentAsync(authorization, cancellationToken) &&
                    await HasScopesAsync(authorization, scopes, cancellationToken))
                {
                    return false;
                }
            }

            return true;
        }

        public virtual async Task<IOpenIdAuthorization> ReplaceAsync(ClaimsPrincipal principal, string client,
            ImmutableArray<string> scopes, ImmutableDictionary<string, string> properties, CancellationToken cancellationToken = default)
        {
            if (principal == null)
            {
                throw new ArgumentNullException(nameof(principal));
            }

            if (string.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client cannot be null or empty.", nameof(client));
            }

            var subject = principal.FindFirstValue(OpenIdConnectConstants.Claims.Subject) ??
                          principal.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(subject))
            {
                throw new InvalidOperationException("The subject claim cannot be extracted from the principal.");
            }

            // Remove all the existing authorizations associated with the user and the client application.
            foreach (var authorization in await FindAsync(subject, client, cancellationToken))
            {
                // If the authorization exactly matches the requested scopes,
                // use it as-is instead of creating a fresh new authorization.
                if (await IsValidAsync(authorization, cancellationToken) &&
                    await IsPermanentAsync(authorization, cancellationToken) &&
                    await HasScopesAsync(authorization, scopes, cancellationToken))
                {
                    return authorization;
                }

                await DeleteAsync(authorization, cancellationToken);
            }

            var descriptor = new OpenIddictAuthorizationDescriptor
            {
                ApplicationId = client,
                Principal = principal,
                Status = OpenIddictConstants.Statuses.Valid,
                Subject = subject,
                Type = OpenIddictConstants.AuthorizationTypes.Permanent
            };

            foreach (var scope in scopes)
            {
                descriptor.Scopes.Add(scope);
            }

            foreach (var property in properties)
            {
                descriptor.Properties.Add(property);
            }

            return await CreateAsync(descriptor, cancellationToken);
        }
    }
}
