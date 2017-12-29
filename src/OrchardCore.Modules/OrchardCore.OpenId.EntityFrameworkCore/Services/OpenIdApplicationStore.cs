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
        /// Determines the number of applications that exist in the database.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of applications in the database.
        /// </returns>
        Task<long> IOpenIddictApplicationStore<IOpenIdApplication>.CountAsync(CancellationToken cancellationToken)
            => CountAsync(cancellationToken);

        /// <summary>
        /// Determines the number of applications that match the specified query.
        /// </summary>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the number of applications that match the specified query.
        /// </returns>
        Task<long> IOpenIddictApplicationStore<IOpenIdApplication>.CountAsync<TResult>(Func<IQueryable<IOpenIdApplication>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => CountAsync(query, cancellationToken);

        /// <summary>
        /// Creates a new application.
        /// </summary>
        /// <param name="application">The application to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result returns the application.
        /// </returns>
        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.CreateAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => await CreateAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Removes an existing application.
        /// </summary>
        /// <param name="application">The application to delete.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictApplicationStore<IOpenIdApplication>.DeleteAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => DeleteAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Retrieves an application using its unique identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client application corresponding to the identifier.
        /// </returns>
        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.FindByIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves an application using its client identifier.
        /// </summary>
        /// <param name="identifier">The client identifier associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client application corresponding to the identifier.
        /// </returns>
        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByClientIdAsync(identifier, cancellationToken);

        /// <summary>
        /// Retrieves all the applications associated with the specified post_logout_redirect_uri.
        /// </summary>
        /// <param name="address">The post_logout_redirect_uri associated with the applications.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result
        /// returns the client applications corresponding to the specified post_logout_redirect_uri.
        /// </returns>
        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.FindByPostLogoutRedirectUriAsync(string address, CancellationToken cancellationToken)
            => (await FindByPostLogoutRedirectUriAsync(address, cancellationToken)).CastArray<IOpenIdApplication>();

        /// <summary>
        /// Retrieves all the applications associated with the specified redirect_uri.
        /// </summary>
        /// <param name="address">The redirect_uri associated with the applications.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result
        /// returns the client applications corresponding to the specified redirect_uri.
        /// </returns>
        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.FindByRedirectUriAsync(string address, CancellationToken cancellationToken)
            => (await FindByRedirectUriAsync(address, cancellationToken)).CastArray<IOpenIdApplication>();

        /// <summary>
        /// Executes the specified query and returns the first element.
        /// </summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="state">The optional state.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the first element returned when executing the query.
        /// </returns>
        Task<TResult> IOpenIddictApplicationStore<IOpenIdApplication>.GetAsync<TState, TResult>(
            Func<IQueryable<IOpenIdApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => GetAsync(query, state, cancellationToken);

        /// <summary>
        /// Retrieves the client identifier associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client identifier associated with the application.
        /// </returns>
        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientIdAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Retrieves the client secret associated with an application.
        /// Note: depending on the manager used to create the application,
        /// the client secret may be hashed for security reasons.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client secret associated with the application.
        /// </returns>
        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientSecretAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientSecretAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Retrieves the client type associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client type of the application (by default, "public").
        /// </returns>
        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetClientTypeAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetClientTypeAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Retrieves the display name associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the display name associated with the application.
        /// </returns>
        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetDisplayNameAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetDisplayNameAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Retrieves the unique identifier associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the unique identifier associated with the application.
        /// </returns>
        Task<string> IOpenIddictApplicationStore<IOpenIdApplication>.GetIdAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetIdAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Retrieves the logout callback addresses associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose
        /// result returns all the post_logout_redirect_uri associated with the application.
        /// </returns>
        Task<ImmutableArray<string>> IOpenIddictApplicationStore<IOpenIdApplication>.GetPostLogoutRedirectUrisAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetPostLogoutRedirectUrisAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Retrieves the callback addresses associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the redirect_uri associated with the application.
        /// </returns>
        Task<ImmutableArray<string>> IOpenIddictApplicationStore<IOpenIdApplication>.GetRedirectUrisAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => GetRedirectUrisAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Instantiates a new application.
        /// </summary>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation, whose result
        /// returns the instantiated application, that can be persisted in the database.
        /// </returns>
        async Task<IOpenIdApplication> IOpenIddictApplicationStore<IOpenIdApplication>.InstantiateAsync(CancellationToken cancellationToken)
            => await InstantiateAsync(cancellationToken);

        /// <summary>
        /// Executes the specified query and returns all the corresponding elements.
        /// </summary>
        /// <param name="count">The number of results to return.</param>
        /// <param name="offset">The number of results to skip.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        async Task<ImmutableArray<IOpenIdApplication>> IOpenIddictApplicationStore<IOpenIdApplication>.ListAsync(int? count, int? offset, CancellationToken cancellationToken)
            => (await ListAsync(count, offset, cancellationToken)).CastArray<IOpenIdApplication>();

        /// <summary>
        /// Executes the specified query and returns all the corresponding elements.
        /// </summary>
        /// <typeparam name="TState">The state type.</typeparam>
        /// <typeparam name="TResult">The result type.</typeparam>
        /// <param name="query">The query to execute.</param>
        /// <param name="state">The optional state.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns all the elements returned when executing the specified query.
        /// </returns>
        Task<ImmutableArray<TResult>> IOpenIddictApplicationStore<IOpenIdApplication>.ListAsync<TState, TResult>(
            Func<IQueryable<IOpenIdApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => ListAsync(query, state, cancellationToken);

        /// <summary>
        /// Sets the client identifier associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="identifier">The client identifier associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientIdAsync(IOpenIdApplication application,
            string identifier, CancellationToken cancellationToken)
            => SetClientIdAsync((OpenIdApplication<TKey>) application, identifier, cancellationToken);

        /// <summary>
        /// Sets the client secret associated with an application.
        /// Note: depending on the manager used to create the application,
        /// the client secret may be hashed for security reasons.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="secret">The client secret associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientSecretAsync(IOpenIdApplication application, string secret, CancellationToken cancellationToken)
            => SetClientSecretAsync((OpenIdApplication<TKey>) application, secret, cancellationToken);

        /// <summary>
        /// Sets the client type associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="type">The client type associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetClientTypeAsync(IOpenIdApplication application, string type, CancellationToken cancellationToken)
            => SetClientTypeAsync((OpenIdApplication<TKey>) application, type, cancellationToken);

        /// <summary>
        /// Sets the display name associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="name">The display name associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetDisplayNameAsync(IOpenIdApplication application, string name, CancellationToken cancellationToken)
            => SetDisplayNameAsync((OpenIdApplication<TKey>) application, name, cancellationToken);

        /// <summary>
        /// Sets the logout callback addresses associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="addresses">The logout callback addresses associated with the application </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetPostLogoutRedirectUrisAsync(IOpenIdApplication application,
            ImmutableArray<string> addresses, CancellationToken cancellationToken)
            => SetPostLogoutRedirectUrisAsync((OpenIdApplication<TKey>) application, addresses, cancellationToken);

        /// <summary>
        /// Sets the callback addresses associated with an application.
        /// </summary>
        /// <param name="application">The application.</param>
        /// <param name="addresses">The callback addresses associated with the application </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictApplicationStore<IOpenIdApplication>.SetRedirectUrisAsync(IOpenIdApplication application,
            ImmutableArray<string> addresses, CancellationToken cancellationToken)
            => SetRedirectUrisAsync((OpenIdApplication<TKey>) application, addresses, cancellationToken);

        /// <summary>
        /// Updates an existing application.
        /// </summary>
        /// <param name="application">The application to update.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        Task IOpenIddictApplicationStore<IOpenIdApplication>.UpdateAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => UpdateAsync((OpenIdApplication<TKey>) application, cancellationToken);

        /// <summary>
        /// Retrieves an application using its physical identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client application corresponding to the identifier.
        /// </returns>
        public virtual async Task<IOpenIdApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base FindByIdAsync() method is called.
            => await FindByIdAsync(identifier, cancellationToken);

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
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync((OpenIdApplication<TKey>) application, cancellationToken);

        // TODO: remove these methods once per-application grant type limitation is added to OpenIddict.
        public virtual Task<ImmutableArray<string>> GetGrantTypesAsync(IOpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var builder = ImmutableArray.CreateBuilder<string>();

            var entity = (OpenIdApplication<TKey>) application;
            if (entity.AllowAuthorizationCodeFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            }

            if (entity.AllowClientCredentialsFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            }

            if (entity.AllowImplicitFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.Implicit);
            }

            if (entity.AllowPasswordFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.Password);
            }

            if (entity.AllowRefreshTokenFlow)
            {
                builder.Add(OpenIdConnectConstants.GrantTypes.RefreshToken);
            }

            return Task.FromResult(builder.ToImmutable());
        }

        public virtual Task SetGrantTypesAsync(IOpenIdApplication application, ImmutableArray<string> types, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var entity = (OpenIdApplication<TKey>) application;

            entity.AllowAuthorizationCodeFlow = types.Contains(OpenIdConnectConstants.GrantTypes.AuthorizationCode);
            entity.AllowClientCredentialsFlow = types.Contains(OpenIdConnectConstants.GrantTypes.ClientCredentials);
            entity.AllowImplicitFlow = types.Contains(OpenIdConnectConstants.GrantTypes.Implicit);
            entity.AllowPasswordFlow = types.Contains(OpenIdConnectConstants.GrantTypes.Password);
            entity.AllowRefreshTokenFlow = types.Contains(OpenIdConnectConstants.GrantTypes.RefreshToken);

            return Task.CompletedTask;
        }

        public virtual Task<bool> IsConsentRequiredAsync(IOpenIdApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Task.FromResult(!((OpenIdApplication<TKey>) application).SkipConsent);
        }

        public virtual Task SetConsentRequiredAsync(IOpenIdApplication application, bool value, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            ((OpenIdApplication<TKey>) application).SkipConsent = !value;

            return Task.CompletedTask;
        }

        // TODO: implement these methods.
        public virtual Task<ImmutableArray<string>> GetRolesAsync(IOpenIdApplication application, CancellationToken cancellationToken)
            => Task.FromResult(ImmutableArray.Create<string>());

        public virtual Task<ImmutableArray<IOpenIdApplication>> ListInRoleAsync(string role, CancellationToken cancellationToken)
            => Task.FromResult(ImmutableArray.Create<IOpenIdApplication>());

        public virtual Task SetRolesAsync(IOpenIdApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }
}
