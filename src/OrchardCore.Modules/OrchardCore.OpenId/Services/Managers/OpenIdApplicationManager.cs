using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.ViewModels;

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

        public virtual async Task<IOpenIdApplication> CreateAsync(
            CreateOpenIdApplicationViewModel model, CancellationToken cancellationToken = default)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            var application = await Store.InstantiateAsync(cancellationToken);
            if (application == null)
            {
                throw new InvalidOperationException("An error occurred while trying to create a new application");
            }

            await Store.SetClientIdAsync(application, model.ClientId, cancellationToken);
            await Store.SetClientSecretAsync(application, model.ClientSecret, cancellationToken);
            await Store.SetClientTypeAsync(application, model.Type.ToString().ToLowerInvariant(), cancellationToken);
            await Store.SetDisplayNameAsync(application, model.DisplayName, cancellationToken);

            await Store.SetConsentTypeAsync(application, model.SkipConsent ?
                OpenIddictConstants.ConsentTypes.Implicit :
                OpenIddictConstants.ConsentTypes.Explicit, cancellationToken);

            if (!string.IsNullOrEmpty(model.LogoutRedirectUri))
            {
                await Store.SetPostLogoutRedirectUrisAsync(application, ImmutableArray.Create(model.LogoutRedirectUri), cancellationToken);
            }

            if (!string.IsNullOrEmpty(model.RedirectUri))
            {
                await Store.SetRedirectUrisAsync(application, ImmutableArray.Create(model.RedirectUri), cancellationToken);
            }

            var permissions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (model.AllowAuthorizationCodeFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            }

            if (model.AllowClientCredentialsFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }

            if (model.AllowImplicitFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
            }

            if (model.AllowPasswordFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
            }

            if (model.AllowRefreshTokenFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }

            if (model.AllowAuthorizationCodeFlow || model.AllowImplicitFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }

            if (model.AllowAuthorizationCodeFlow || model.AllowClientCredentialsFlow ||
                model.AllowPasswordFlow || model.AllowRefreshTokenFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
            }

            await Store.SetPermissionsAsync(application, permissions.ToImmutableArray(), cancellationToken);

            await Store.SetRolesAsync(application, model.RoleEntries
                .Where(role => role.Selected)
                .Select(role => role.Name)
                .ToImmutableArray(), cancellationToken);

            var secret = await Store.GetClientSecretAsync(application, cancellationToken);
            if (!string.IsNullOrEmpty(secret))
            {
                await Store.SetClientSecretAsync(application, /* secret: */ null, cancellationToken);
                await CreateAsync(application, secret, cancellationToken);
            }
            else
            {
                await CreateAsync(application, cancellationToken);
            }

            return application;
        }

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

        public virtual async Task UpdateAsync(IOpenIdApplication application,
            EditOpenIdApplicationViewModel model, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }

            await Store.SetClientIdAsync(application, model.ClientId, cancellationToken);
            await Store.SetClientTypeAsync(application, model.Type.ToString().ToLowerInvariant(), cancellationToken);
            await Store.SetDisplayNameAsync(application, model.DisplayName, cancellationToken);

            await Store.SetConsentTypeAsync(application, model.SkipConsent ?
                OpenIddictConstants.ConsentTypes.Implicit :
                OpenIddictConstants.ConsentTypes.Explicit, cancellationToken);

            var permissions = new HashSet<string>(await Store.GetPermissionsAsync(application, cancellationToken), StringComparer.OrdinalIgnoreCase);

            if (model.AllowAuthorizationCodeFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            }
            else
            {
                permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode);
            }

            if (model.AllowClientCredentialsFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }
            else
            {
                permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.ClientCredentials);
            }

            if (model.AllowImplicitFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Implicit);
            }
            else
            {
                permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.Implicit);
            }

            if (model.AllowPasswordFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.Password);
            }
            else
            {
                permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.Password);
            }

            if (model.AllowRefreshTokenFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }
            else
            {
                permissions.Remove(OpenIddictConstants.Permissions.GrantTypes.RefreshToken);
            }

            if (model.AllowAuthorizationCodeFlow || model.AllowImplicitFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }
            else
            {
                permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Authorization);
            }

            if (model.AllowAuthorizationCodeFlow || model.AllowClientCredentialsFlow ||
                model.AllowPasswordFlow || model.AllowRefreshTokenFlow)
            {
                permissions.Add(OpenIddictConstants.Permissions.Endpoints.Token);
            }
            else
            {
                permissions.Remove(OpenIddictConstants.Permissions.Endpoints.Token);
            }

            await Store.SetPermissionsAsync(application, permissions.ToImmutableArray(), cancellationToken);

            await Store.SetRolesAsync(application, model.RoleEntries
                .Where(role => role.Selected)
                .Select(role => role.Name)
                .ToImmutableArray(), cancellationToken);

            if (!string.IsNullOrEmpty(model.LogoutRedirectUri))
            {
                await Store.SetPostLogoutRedirectUrisAsync(application, ImmutableArray.Create(model.LogoutRedirectUri), cancellationToken);
            }
            else
            {
                await Store.SetPostLogoutRedirectUrisAsync(application, ImmutableArray.Create<string>(), cancellationToken);
            }

            if (!string.IsNullOrEmpty(model.RedirectUri))
            {
                await Store.SetRedirectUrisAsync(application, ImmutableArray.Create(model.RedirectUri), cancellationToken);
            }
            else
            {
                await Store.SetRedirectUrisAsync(application, ImmutableArray.Create<string>(), cancellationToken);
            }

            if (model.UpdateClientSecret)
            {
                await UpdateAsync(application, model.ClientSecret, cancellationToken);
            }
            else
            {
                await UpdateAsync(application, cancellationToken);
            }
        }
    }
}
