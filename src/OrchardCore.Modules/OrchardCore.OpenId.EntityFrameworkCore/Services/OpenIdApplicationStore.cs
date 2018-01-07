using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.EntityFrameworkCore;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore;
using OrchardCore.OpenId.Abstractions.Models;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.EntityFrameworkCore.Models;

namespace OrchardCore.OpenId.EntityFrameworkCore.Services
{
    public class OpenIdApplicationStore<TContext, TKey> : OpenIddictApplicationStore<OpenIdApplication<TKey>, OpenIdAuthorization<TKey>, OpenIdToken<TKey>, TContext, TKey>, IOpenIdApplicationStore
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdApplicationStore(TContext context)
            : base(context)
        {
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
        public virtual Task<OpenIdApplication<TKey>> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base FindByIdAsync() method is called.
            => FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves the physical identifier associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the application.
        /// </returns>
        public virtual Task<string> GetPhysicalIdAsync(OpenIdApplication<TKey> application, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync(application, cancellationToken);

        // TODO: remove these methods once per-application grant type limitation is added to OpenIddict.
        public virtual Task<ImmutableArray<string>> GetGrantTypesAsync(OpenIdApplication<TKey> application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var builder = ImmutableArray.CreateBuilder<string>();

            if (application.AllowAuthorizationCodeFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            }

            if (application.AllowClientCredentialsFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            }

            if (application.AllowImplicitFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.Implicit);
            }

            if (application.AllowPasswordFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.Password);
            }

            if (application.AllowRefreshTokenFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.RefreshToken);
            }

            return Task.FromResult(builder.ToImmutable());
        }

        public virtual Task SetGrantTypesAsync(OpenIdApplication<TKey> application, ImmutableArray<string> types, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.AllowAuthorizationCodeFlow = types.Contains(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            application.AllowClientCredentialsFlow = types.Contains(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            application.AllowImplicitFlow = types.Contains(OpenIdConnectConstants.GrantTypes.Implicit);
            application.AllowPasswordFlow = types.Contains(OpenIdConnectConstants.GrantTypes.Password);
            application.AllowRefreshTokenFlow = types.Contains(OpenIdConnectConstants.GrantTypes.RefreshToken);

            return Task.CompletedTask;
        }

        public virtual Task<bool> IsConsentRequiredAsync(OpenIdApplication<TKey> application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(!application.SkipConsent);
        }

        public virtual Task SetConsentRequiredAsync(OpenIdApplication<TKey> application, bool value, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.SkipConsent = !value;

            return Task.CompletedTask;
        }

        // TODO: implement these methods.
        public virtual Task<ImmutableArray<string>> GetRolesAsync(OpenIdApplication<TKey> application, CancellationToken cancellationToken)
            => Task.FromResult(ImmutableArray.Create<string>());

        public virtual Task<ImmutableArray<OpenIdApplication<TKey>>> ListInRoleAsync(string role, CancellationToken cancellationToken)
            => Task.FromResult(ImmutableArray.Create<OpenIdApplication<TKey>>());

        public virtual Task SetRolesAsync(OpenIdApplication<TKey> application, ImmutableArray<string> roles, CancellationToken cancellationToken)
            => Task.CompletedTask;

        // Note: the following methods are deliberately implemented as explicit methods so they are not
        // exposed by Intellisense. Their logic MUST be limited to dealing with casts and downcasts.
        // Developers who need to customize the logic SHOULD override the methods taking concretes types.

        // -------------------------------------------------------------
        // Methods defined by the IOpenIddictApplicationStore interface:
        // -------------------------------------------------------------

        Task<long> IOpenIddictApplicationStore<IOpenIdApplication>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        Task<long> IOpenIddictApplicationStore<IOpenIdApplication>.CountAsync<TResult>(Func<IQueryable<IOpenIdApplication>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.CreateAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => await CreateAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.DeleteAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdApplication<TKey>) application, cancellationToken);

        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByClientIdAsync(identifier, cancellationToken);

        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.FindByPostLogoutRedirectUriAsync(string address, CancellationToken cancellationToken)
            => (await FindByPostLogoutRedirectUriAsync(address, cancellationToken)).CastArray<IOpenIdApplication>();

        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.FindByRedirectUriAsync(string address, CancellationToken cancellationToken)
            => (await FindByRedirectUriAsync(address, cancellationToken)).CastArray<IOpenIdApplication>();

        Task<TResult> IOpenIddictApplicationStore<IOpenIdApplication>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientIdAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientSecretAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientSecretAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientTypeAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientTypeAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetDisplayNameAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetDisplayNameAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<ImmutableArray<string>> IOpenIddictApplicationStore<IOpenIdApplication>.GetPostLogoutRedirectUrisAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetPostLogoutRedirectUrisAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<ImmutableArray<string>> IOpenIddictApplicationStore<IOpenIdApplication>.GetRedirectUrisAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetRedirectUrisAsync((OpenIdApplication<TKey>) application, cancellationToken);

        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdApplication>();

        Task<ImmutableArray<TResult>> IOpenIddictApplicationStore<IOpenIdApplication>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientIdAsync(IOpenIdApplication application,
            string identifier, CancellationToken cancellationToken)
            => SetClientIdAsync((OpenIdApplication<TKey>) application, identifier, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientSecretAsync(IOpenIdApplication application, string secret, CancellationToken cancellationToken)
            => SetClientSecretAsync((OpenIdApplication<TKey>) application, secret, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientTypeAsync(IOpenIdApplication application, string type, CancellationToken cancellationToken)
            => SetClientTypeAsync((OpenIdApplication<TKey>) application, type, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetDisplayNameAsync(IOpenIdApplication application, string name, CancellationToken cancellationToken)
            => SetDisplayNameAsync((OpenIdApplication<TKey>) application, name, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetPostLogoutRedirectUrisAsync(IOpenIdApplication application,
            ImmutableArray<string> addresses, CancellationToken cancellationToken)
            => SetPostLogoutRedirectUrisAsync((OpenIdApplication<TKey>) application, addresses, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetRedirectUrisAsync(IOpenIdApplication application,
            ImmutableArray<string> addresses, CancellationToken cancellationToken)
            => SetRedirectUrisAsync((OpenIdApplication<TKey>) application, addresses, cancellationToken);

        Task IOpenIddictApplicationStore<IOpenIdApplication>.UpdateAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdApplication<TKey>) application, cancellationToken);

        // ---------------------------------------------------------
        // Methods defined by the IOpenIdApplicationStore interface:
        // ---------------------------------------------------------

        async Task<IOpenIdApplication> IOpenIdApplicationStore.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        Task<ImmutableArray<string>> IOpenIdApplicationStore.GetGrantTypesAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetGrantTypesAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<string> IOpenIdApplicationStore.GetPhysicalIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<ImmutableArray<string>> IOpenIdApplicationStore.GetRolesAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetRolesAsync((OpenIdApplication<TKey>) application, cancellationToken);

        Task<bool> IOpenIdApplicationStore.IsConsentRequiredAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => IsConsentRequiredAsync((OpenIdApplication<TKey>) application, cancellationToken);

        async Task<ImmutableArray<IOpenIdApplication>> IOpenIdApplicationStore.ListInRoleAsync(string role, CancellationToken cancellationToken)
            => (await ListInRoleAsync(role, cancellationToken)).CastArray<IOpenIdApplication>();

        Task IOpenIdApplicationStore.SetConsentRequiredAsync(IOpenIdApplication application, bool value, CancellationToken cancellationToken)
            => SetConsentRequiredAsync((OpenIdApplication<TKey>) application, value, cancellationToken);

        Task IOpenIdApplicationStore.SetGrantTypesAsync(IOpenIdApplication application, ImmutableArray<string> types, CancellationToken cancellationToken)
            => SetGrantTypesAsync((OpenIdApplication<TKey>) application, types, cancellationToken);

        Task IOpenIdApplicationStore.SetRolesAsync(IOpenIdApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken)
            => SetRolesAsync((OpenIdApplication<TKey>) application, roles, cancellationToken);
    }
}
