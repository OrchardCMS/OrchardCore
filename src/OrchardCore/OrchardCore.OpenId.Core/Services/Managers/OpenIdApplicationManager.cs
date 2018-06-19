using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OrchardCore.OpenId.Abstractions.Descriptors;
using OrchardCore.OpenId.Abstractions.Managers;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.Services.Managers
{
    public class OpenIdApplicationManager<TApplication> : OpenIddictApplicationManager<TApplication>,
        IOpenIdApplicationManager where TApplication : class
    {
        public OpenIdApplicationManager(
            IOpenIddictApplicationStoreResolver resolver,
            ILogger<OpenIdApplicationManager<TApplication>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options)
            : base(resolver, logger, options)
        {
        }

        protected new IOpenIdApplicationStore<TApplication> Store => (IOpenIdApplicationStore<TApplication>) base.Store;

        /// <summary>
        /// Retrieves an application using its physical identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="Task"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client application corresponding to the identifier.
        /// </returns>
        public virtual Task<TApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
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
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the application.
        /// </returns>
        public virtual ValueTask<string> GetPhysicalIdAsync(TApplication application, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Store.GetPhysicalIdAsync(application, cancellationToken);
        }

        public virtual async Task AddToRoleAsync(TApplication application,
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

        public virtual ValueTask<ImmutableArray<string>> GetRolesAsync(
            TApplication application, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return Store.GetRolesAsync(application, cancellationToken);
        }

        public virtual async Task<bool> IsInRoleAsync(TApplication application,
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

        public virtual Task<ImmutableArray<TApplication>> ListInRoleAsync(
            string role, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            return Store.ListInRoleAsync(role, cancellationToken);
        }

        public virtual async Task RemoveFromRoleAsync(TApplication application,
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

        public override async Task PopulateAsync(TApplication application,
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

            if (descriptor is OpenIdApplicationDescriptor model)
            {
                await Store.SetRolesAsync(application, model.Roles.ToImmutableArray(), cancellationToken);
            }

            await base.PopulateAsync(application, descriptor, cancellationToken);
        }

        public override async Task PopulateAsync(OpenIddictApplicationDescriptor descriptor,
            TApplication application, CancellationToken cancellationToken = default)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (descriptor is OpenIdApplicationDescriptor model)
            {
                model.Roles.UnionWith(await Store.GetRolesAsync(application, cancellationToken));
            }

            await base.PopulateAsync(descriptor, application, cancellationToken);
        }

        Task IOpenIdApplicationManager.AddToRoleAsync(object application, string role, CancellationToken cancellationToken)
            => AddToRoleAsync((TApplication) application, role, cancellationToken);

        async Task<object> IOpenIdApplicationManager.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        ValueTask<string> IOpenIdApplicationManager.GetPhysicalIdAsync(object application, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TApplication) application, cancellationToken);

        ValueTask<ImmutableArray<string>> IOpenIdApplicationManager.GetRolesAsync(object application, CancellationToken cancellationToken)
            => GetRolesAsync((TApplication) application, cancellationToken);

        Task<bool> IOpenIdApplicationManager.IsInRoleAsync(object application, string role, CancellationToken cancellationToken)
            => IsInRoleAsync((TApplication) application, role, cancellationToken);

        async Task<ImmutableArray<object>> IOpenIdApplicationManager.ListInRoleAsync(string role, CancellationToken cancellationToken)
            => (await ListInRoleAsync(role, cancellationToken)).CastArray<object>();

        Task IOpenIdApplicationManager.RemoveFromRoleAsync(object application, string role, CancellationToken cancellationToken)
            => RemoveFromRoleAsync((TApplication) application, role, cancellationToken);
    }
}
