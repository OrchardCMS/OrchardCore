using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Encodings.Web;
using System.Text.Json;
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
            IOpenIddictApplicationCache<TApplication> cache,
            ILogger<OpenIdApplicationManager<TApplication>> logger,
            IOptionsMonitor<OpenIddictCoreOptions> options,
            IOpenIddictApplicationStoreResolver resolver)
            : base(cache, logger, options, resolver)
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
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            return Store is IOpenIdApplicationStore<TApplication> store ?
                store.FindByPhysicalIdAsync(identifier, cancellationToken) :
                Store.FindByIdAsync(identifier, cancellationToken);
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
                if (properties.TryGetValue(OpenIdConstants.Properties.Roles, out var value))
                {
                    var builder = ImmutableArray.CreateBuilder<string>();

                    foreach (var item in value.EnumerateArray())
                    {
                        builder.Add(item.GetString());
                    }

                    return builder.ToImmutable();
                }

                return ImmutableArray.Create<string>();
            }
        }

        public virtual IAsyncEnumerable<TApplication> ListInRoleAsync(
            string role, CancellationToken cancellationToken = default)
        {
            if (String.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            if (Store is IOpenIdApplicationStore<TApplication> store)
            {
                return store.ListInRoleAsync(role, cancellationToken);
            }

            return ExecuteAsync();

            async IAsyncEnumerable<TApplication> ExecuteAsync()
            {
                for (var offset = 0; ; offset += 1_000)
                {
                    await foreach (var application in Store.ListAsync(1_000, offset, cancellationToken))
                    {
                        var roles = await GetRolesAsync(application, cancellationToken);
                        if (roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                        {
                            yield return application;
                        }
                    }
                }
            }
        }

        public virtual async ValueTask SetRolesAsync(TApplication application,
            ImmutableArray<string> roles, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (roles.Any(role => String.IsNullOrEmpty(role)))
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
                properties = properties.SetItem(OpenIdConstants.Properties.Roles,
                    JsonSerializer.SerializeToElement(roles, new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    }));

                await Store.SetPropertiesAsync(application, properties, cancellationToken);
            }

            await UpdateAsync(application, cancellationToken);
        }

        public override async ValueTask PopulateAsync(TApplication application,
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

            // Note: this method MUST be called first before applying any change to the untyped
            // properties bag to ensure the base method doesn't override the added properties.
            await base.PopulateAsync(application, descriptor, cancellationToken);

            if (descriptor is OpenIdApplicationDescriptor model)
            {
                // If the underlying store is an Orchard implementation that natively supports roles,
                // use the corresponding API. Otherwise, store the roles in the untyped properties bag.
                if (Store is IOpenIdApplicationStore<TApplication> store)
                {
                    await store.SetRolesAsync(application, model.Roles.ToImmutableArray(), cancellationToken);
                }
                else
                {
                    var properties = await Store.GetPropertiesAsync(application, cancellationToken);
                    properties = properties.SetItem(OpenIdConstants.Properties.Roles,
                        JsonSerializer.SerializeToElement(model.Roles, new JsonSerializerOptions
                        {
                            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        }));

                    await Store.SetPropertiesAsync(application, properties, cancellationToken);
                }
            }
        }

        public override async ValueTask PopulateAsync(OpenIddictApplicationDescriptor descriptor,
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

            await base.PopulateAsync(descriptor, application, cancellationToken);

            if (descriptor is OpenIdApplicationDescriptor model)
            {
                model.Roles.UnionWith(await GetRolesAsync(application, cancellationToken));
            }
        }

        public override IAsyncEnumerable<ValidationResult> ValidateAsync(
            TApplication application, CancellationToken cancellationToken = default)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return ExecuteAsync();

            async IAsyncEnumerable<ValidationResult> ExecuteAsync()
            {
                await foreach (var result in base.ValidateAsync(application, cancellationToken))
                {
                    yield return result;
                }

                foreach (var role in await GetRolesAsync(application, cancellationToken))
                {
                    if (String.IsNullOrEmpty(role))
                    {
                        yield return new ValidationResult("Roles cannot be null or empty.");

                        break;
                    }
                }
            }
        }

        async ValueTask<object> IOpenIdApplicationManager.FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
            => await FindByPhysicalIdAsync(identifier, cancellationToken);

        ValueTask<string> IOpenIdApplicationManager.GetPhysicalIdAsync(object application, CancellationToken cancellationToken)
            => GetPhysicalIdAsync((TApplication)application, cancellationToken);

        ValueTask<ImmutableArray<string>> IOpenIdApplicationManager.GetRolesAsync(object application, CancellationToken cancellationToken)
            => GetRolesAsync((TApplication)application, cancellationToken);

        IAsyncEnumerable<object> IOpenIdApplicationManager.ListInRoleAsync(string role, CancellationToken cancellationToken)
            => ListInRoleAsync(role, cancellationToken);

        ValueTask IOpenIdApplicationManager.SetRolesAsync(object application, ImmutableArray<string> roles, CancellationToken cancellationToken)
            => SetRolesAsync((TApplication)application, roles, cancellationToken);
    }
}
