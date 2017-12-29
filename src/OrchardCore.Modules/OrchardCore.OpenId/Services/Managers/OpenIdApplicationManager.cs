using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.Models;
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

        public virtual async Task<IOpenIdApplication> CreateAsync(CreateOpenIdApplicationViewModel model, CancellationToken cancellationToken)
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
            await Store.SetClientTypeAsync(application, model.Type.ToString(), cancellationToken);
            await Store.SetDisplayNameAsync(application, model.DisplayName, cancellationToken);
            await Store.SetRolesAsync(application, model.RoleEntries.Select(role => role.Name).ToImmutableArray(), cancellationToken);

            if (!string.IsNullOrEmpty(model.LogoutRedirectUri))
            {
                await Store.SetPostLogoutRedirectUrisAsync(application, ImmutableArray.Create(model.LogoutRedirectUri), cancellationToken);
            }

            if (!string.IsNullOrEmpty(model.RedirectUri))
            {
                await Store.SetRedirectUrisAsync(application, ImmutableArray.Create(model.RedirectUri), cancellationToken);
            }

            var builder = ImmutableArray.CreateBuilder<string>();

            if (model.AllowAuthorizationCodeFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            }

            if (model.AllowClientCredentialsFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            }

            if (model.AllowImplicitFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.Implicit);
            }

            if (model.AllowPasswordFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.Password);
            }

            if (model.AllowRefreshTokenFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.RefreshToken);
            }

            await Store.SetGrantTypesAsync(application, builder.ToImmutable(), cancellationToken);

            var secret = await Store.GetClientSecretAsync(application, cancellationToken);
            if (!string.IsNullOrEmpty(secret))
            {
                await Store.SetClientSecretAsync(application, /* secret: */ null, cancellationToken);
                return await CreateAsync(application, secret, cancellationToken);
            }

            return await CreateAsync(application, cancellationToken);
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
        public virtual Task<IOpenIdApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return Store.FindByPhysicalIdAsync(identifier, cancellationToken);
        }

        public virtual new async Task<ClientType> GetClientTypeAsync(IOpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (!Enum.TryParse(await Store.GetClientTypeAsync(application, cancellationToken), /* ignoreCase: */ true, out ClientType type))
            {
                throw new InvalidOperationException("The client type associated with the application is not supported.");
            }

            return type;
        }

        // TODO: remove these methods once per-application grant type limitation is added to OpenIddict.
        public virtual Task<ImmutableArray<string>> GetGrantTypesAsync(IOpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Store.GetGrantTypesAsync(application, cancellationToken);
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
        public virtual Task<string> GetPhysicalIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Store.GetPhysicalIdAsync(application, cancellationToken);
        }

        public virtual async Task<bool> IsGrantTypeAllowedAsync(IOpenIdApplication application, string type, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return (await Store.GetGrantTypesAsync(application, cancellationToken)).Contains(type, StringComparer.OrdinalIgnoreCase);
        }

        public Task<bool> IsAuthorizationCodeFlowAllowedAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => IsGrantTypeAllowedAsync(application, OpenIdConnectConstants.GrantTypes.AuthorizationCode, cancellationToken);

        public Task<bool> IsClientCredentialsFlowAllowedAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => IsGrantTypeAllowedAsync(application, OpenIdConnectConstants.GrantTypes.ClientCredentials, cancellationToken);

        public async Task<bool> IsHybridFlowAllowedAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => await IsAuthorizationCodeFlowAllowedAsync(application, cancellationToken) &&
               await IsImplicitFlowAllowedAsync(application, cancellationToken);

        public Task<bool> IsImplicitFlowAllowedAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => IsGrantTypeAllowedAsync(application, OpenIdConnectConstants.GrantTypes.Implicit, cancellationToken);

        public Task<bool> IsPasswordFlowAllowedAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => IsGrantTypeAllowedAsync(application, OpenIdConnectConstants.GrantTypes.Password, cancellationToken);

        public Task<bool> IsRefreshTokenFlowAllowedAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => IsGrantTypeAllowedAsync(application, OpenIdConnectConstants.GrantTypes.RefreshToken, cancellationToken);

        public virtual Task<bool> IsConsentRequiredAsync(IOpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Store.IsConsentRequiredAsync(application, cancellationToken);
        }

        public virtual async Task AddToRoleAsync(IOpenIdApplication application, string role, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var roles = await Store.GetRolesAsync(application, cancellationToken);
            await Store.SetRolesAsync(application, roles.Add(role), cancellationToken);
            await UpdateAsync(application, cancellationToken);
        }

        public virtual Task<ImmutableArray<string>> GetRolesAsync(IOpenIdApplication application, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Store.GetRolesAsync(application, cancellationToken);
        }

        public virtual async Task<bool> IsInRoleAsync(IOpenIdApplication application, string role, CancellationToken cancellationToken = default(CancellationToken))
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

        public virtual Task<ImmutableArray<IOpenIdApplication>> ListInRoleAsync(string role, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            return Store.ListInRoleAsync(role, cancellationToken);
        }

        public virtual async Task RemoveFromRoleAsync(IOpenIdApplication application, string role, CancellationToken cancellationToken = default(CancellationToken))
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

        public virtual async Task UpdateAsync(IOpenIdApplication application, EditOpenIdApplicationViewModel model, CancellationToken cancellationToken)
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
            await Store.SetClientTypeAsync(application, model.Type.ToString(), cancellationToken);
            await Store.SetDisplayNameAsync(application, model.DisplayName, cancellationToken);
            await Store.SetRolesAsync(application, model.RoleEntries.Select(role => role.Name).ToImmutableArray(), cancellationToken);

            var builder = ImmutableArray.CreateBuilder<string>();

            if (model.AllowAuthorizationCodeFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            }

            if (model.AllowClientCredentialsFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            }

            if (model.AllowImplicitFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.Implicit);
            }

            if (model.AllowPasswordFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.Password);
            }

            if (model.AllowRefreshTokenFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.RefreshToken);
            }

            await Store.SetGrantTypesAsync(application, builder.ToImmutable(), cancellationToken);

            if (!string.IsNullOrEmpty(model.LogoutRedirectUri))
            {
                await Store.SetPostLogoutRedirectUrisAsync(application, ImmutableArray.Create(model.LogoutRedirectUri), cancellationToken);
            }

            if (!string.IsNullOrEmpty(model.RedirectUri))
            {
                await Store.SetRedirectUrisAsync(application, ImmutableArray.Create(model.RedirectUri), cancellationToken);
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
