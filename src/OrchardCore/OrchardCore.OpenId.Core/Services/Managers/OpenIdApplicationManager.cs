using System;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
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
            IOpenIddictApplicationCache<TApplication> cache,
            IOpenIddictApplicationStoreResolver resolver,
            ILogger<OpenIdApplicationManager<TApplication>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options)
            : base(cache, resolver, logger, options)
        {
        }

        /// <summary>
        /// Retrieves an application using its physical identifier.
        /// </summary>
        /// <param name="identifier">The unique identifier associated with the application.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the client application corresponding to the identifier.
        /// </returns>
        public virtual ValueTask<TApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return new ValueTask<TApplication>(Store is IOpenIdApplicationStore<TApplication> store ?
                store.FindByPhysicalIdAsync(identifier, cancellationToken) :
                Store.FindByIdAsync(identifier, cancellationToken));
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

            return Store is IOpenIdApplicationStore<TApplication> store ?
                store.GetPhysicalIdAsync(application, cancellationToken) :
                Store.GetIdAsync(application, cancellationToken);
        }

        public virtual async ValueTask<ImmutableArray<string>> GetRolesAsync(
            TApplication application, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (Store is IOpenIdApplicationStore<TApplication> store)
            {
                return await store.GetRolesAsync(application, cancellationToken);
            }
            else
            {
                var properties = await Store.GetPropertiesAsync(application, cancellationToken);
                if (properties.TryGetValue(OpenIdConstants.Properties.Roles, StringComparison.OrdinalIgnoreCase, out JToken value))
                {
                    return value.ToObject<ImmutableArray<string>>();
                }

                return ImmutableArray.Create<string>();
            }
        }

        public virtual async ValueTask<ImmutableArray<TApplication>> ListInRoleAsync(
            string role, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            if (Store is IOpenIdApplicationStore<TApplication> store)
            {
                return await store.ListInRoleAsync(role, cancellationToken);
            }

            var builder = ImmutableArray.CreateBuilder<TApplication>();

            for (var offset = 0; ; offset += 1_000)
            {
                var applications = await Store.ListAsync(1_000, offset, cancellationToken);
                if (applications.Length == 0)
                {
                    break;
                }

                foreach (var application in applications)
                {
                    var roles = await GetRolesAsync(application, cancellationToken);
                    if (roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                    {
                        builder.Add(application);
                    }
                }
            }

            return builder.ToImmutable();
        }

        public virtual async ValueTask SetRolesAsync(TApplication application,
            ImmutableArray<string> roles, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (roles.Any(role => string.IsNullOrEmpty(role)))
            {
                throw new ArgumentException("Role names cannot be null or empty.", nameof(roles));
            }

            if (Store is IOpenIdApplicationStore<TApplication> store)
            {
                await store.SetRolesAsync(application, roles, cancellationToken);
            }
            else
            {
                var properties = await Store.GetPropertiesAsync(application, cancellationToken);
                properties[OpenIdConstants.Properties.Roles] = JArray.FromObject(roles);

                await Store.SetPropertiesAsync(application, properties, cancellationToken);
            }

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
                if (Store is IOpenIdApplicationStore<TApplication> store)
                {
                    await store.SetRolesAsync(application, model.Roles.ToImmutableArray(), cancellationToken);
                }
                else
                {
                    var properties = await Store.GetPropertiesAsync(application, cancellationToken);
                    properties[OpenIdConstants.Properties.Roles] = JArray.FromObject(model.Roles);

                    await Store.SetPropertiesAsync(application, properties, cancellationToken);
                }
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
                model.Roles.UnionWith(await GetRolesAsync(application, cancellationToken));
            }

            await base.PopulateAsync(descriptor, application, cancellationToken);
        }

        public override async Task<ImmutableArray<ValidationResult>> ValidateAsync(
            TApplication application, CancellationToken cancellationToken = default)
        {
            var results = ImmutableArray.CreateBuilder<ValidationResult>();
            results.AddRange(await base.ValidateAsync(application, cancellationToken));

            foreach (var role in await GetRolesAsync(application, cancellationToken))
            {
                if (string.IsNullOrEmpty(role))
                {
                    results.Add(new ValidationResult("Roles cannot be null or empty."));

                    break;
                }
            }

            return results.ToImmutable();
        }

        async ValueTask<object> IOpenIdApplicationManager.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        ValueTask<string> IOpenIdApplicationManager.GetPhysicalIdAsync(object application, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TApplication)application, cancellationToken);

        ValueTask<ImmutableArray<string>> IOpenIdApplicationManager.GetRolesAsync(object application, CancellationToken cancellationToken)
            => GetRolesAsync((TApplication)application, cancellationToken);

        async ValueTask<ImmutableArray<object>> IOpenIdApplicationManager.ListInRoleAsync(string role, CancellationToken cancellationToken)
            => (await ListInRoleAsync(role, cancellationToken)).CastArray<object>();

        ValueTask IOpenIdApplicationManager.SetRolesAsync(object application, ImmutableArray<string> roles, CancellationToken cancellationToken)
            => SetRolesAsync((TApplication)application, roles, cancellationToken);
    }
}
