using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using OpenIddict.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using OrchardCore.OpenId.Abstractions.Stores;

namespace OrchardCore.OpenId.EntityFrameworkCore.Services
{
    public class OpenIdApplicationStore<TContext, TKey> : OpenIdApplicationStore<OpenIddictApplication<TKey>,
                                                                                 OpenIddictAuthorization<TKey>,
                                                                                 OpenIddictToken<TKey>, TContext, TKey>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdApplicationStore(
            IMemoryCache cache,
            TContext context,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
            : base(cache, context, options)
        {
        }
    }

    public class OpenIdApplicationStore<TApplication, TAuthorization, TToken, TContext, TKey> :
        OpenIddictApplicationStore<TApplication, TAuthorization, TToken, TContext, TKey>, IOpenIdApplicationStore<TApplication>
        where TApplication : OpenIddictApplication<TKey, TAuthorization, TToken>, new()
        where TAuthorization : OpenIddictAuthorization<TKey, TApplication, TToken>, new()
        where TToken : OpenIddictToken<TKey, TApplication, TAuthorization>, new()
        where TContext : DbContext
        where TKey : IEquatable<TKey>
    {
        public OpenIdApplicationStore(
            IMemoryCache cache,
            TContext context,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
            : base(cache, context, options)
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
        public virtual Task<TApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
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
        /// A <see cref="ValueTask{TResult}"/> that can be used to monitor the asynchronous operation,
        /// whose result returns the physical identifier associated with the application.
        /// </returns>
        public virtual ValueTask<string> GetPhysicalIdAsync(TApplication application, CancellationToken cancellationToken)
            // Note: unlike the YesSql-specific models, the default OpenIddict models used by
            // the Entity Framework Core stores don't have distinct physical/logical identifiers.
            // To ensure this method can be safely used, the base GetIdAsync() method is called.
            => GetIdAsync(application, cancellationToken);

        public virtual async ValueTask<ImmutableArray<string>> GetRolesAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var properties = await GetPropertiesAsync(application, cancellationToken);
            if (properties.TryGetValue(OpenIdConstants.Properties.Roles, StringComparison.OrdinalIgnoreCase, out JToken value))
            {
                return value.ToObject<string[]>().ToImmutableArray();
            }

            return ImmutableArray.Create<string>();
        }

        public virtual async Task<ImmutableArray<TApplication>> ListInRoleAsync(string role, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            var builder = ImmutableArray.CreateBuilder<TApplication>();

            // To optimize the efficiency of the query a bit, only applications whose stringified
            // Properties column contains the specified role are returned. Once the applications
            // are retrieved, a second pass is made to ensure only valid elements are returned.
            // Implementers that use this method in a hot path may want to override this method
            // to use SQL Server 2016 functions like JSON_VALUE to make the query more efficient.
            foreach (var application in await (from application in Context.Set<TApplication>()
                                               where application.Properties.Contains(role)
                                               select application).ToListAsync())
            {
                var roles = await GetRolesAsync(application, cancellationToken);
                if (roles.Contains(role, StringComparer.OrdinalIgnoreCase))
                {
                    builder.Add(application);
                }
            }

            return builder.ToImmutable();
        }

        public virtual async Task SetRolesAsync(TApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            var properties = await GetPropertiesAsync(application, cancellationToken);
            properties[OpenIdConstants.Properties.Roles] = new JArray(roles.ToArray());

            await SetPropertiesAsync(application, properties, cancellationToken);
        }
    }
}
