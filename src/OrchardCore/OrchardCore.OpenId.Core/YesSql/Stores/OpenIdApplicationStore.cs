using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using YesSql;

namespace OrchardCore.OpenId.YesSql.Stores
{
    public class OpenIdApplicationStore<TApplication> : IOpenIdApplicationStore<TApplication>
        where TApplication : OpenIdApplication, new()
    {
        private const string OpenIdCollection = OpenIdAuthorization.OpenIdCollection;
        private readonly ISession _session;

        public OpenIdApplicationStore(ISession session)
        {
            _session = session;
        }

        /// <inheritdoc/>
        public virtual async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<TApplication>(collection: OpenIdCollection).CountAsync();
        }

        /// <inheritdoc/>
        public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<TApplication>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public virtual async ValueTask CreateAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(application, collection: OpenIdCollection);
            await _session.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual async ValueTask DeleteAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Delete(application, collection: OpenIdCollection);
            await _session.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual async ValueTask<TApplication> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<TApplication, OpenIdApplicationIndex>(index => index.ApplicationId == identifier, collection: OpenIdCollection).FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public virtual async ValueTask<TApplication> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<TApplication, OpenIdApplicationIndex>(index => index.ClientId == identifier, collection: OpenIdCollection).FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public virtual async ValueTask<TApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.GetAsync<TApplication>(Int64.Parse(identifier, CultureInfo.InvariantCulture), collection: OpenIdCollection);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TApplication> FindByPostLogoutRedirectUriAsync(string uri, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("The URI cannot be null or empty.", nameof(uri));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<TApplication, OpenIdAppByLogoutUriIndex>(
                index => index.LogoutRedirectUri == uri,
                collection: OpenIdCollection).ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TApplication> FindByRedirectUriAsync(string uri, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(uri))
            {
                throw new ArgumentException("The URI cannot be null or empty.", nameof(uri));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<TApplication, OpenIdAppByRedirectUriIndex>(
                index => index.RedirectUri == uri,
                collection: OpenIdCollection).ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual ValueTask<TResult> GetAsync<TState, TResult>(
            Func<IQueryable<TApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public virtual ValueTask<string> GetClientIdAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string>(application.ClientId);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetClientSecretAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string>(application.ClientSecret);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetClientTypeAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string>(application.Type);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetConsentTypeAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string>(application.ConsentType);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetDisplayNameAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string>(application.DisplayName);
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(
            TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (application.DisplayNames == null)
            {
                return new ValueTask<ImmutableDictionary<CultureInfo, string>>(ImmutableDictionary.Create<CultureInfo, string>());
            }

            return new ValueTask<ImmutableDictionary<CultureInfo, string>>(application.DisplayNames);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetIdAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string>(application.ApplicationId);
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableArray<string>> GetPermissionsAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<ImmutableArray<string>>(application.Permissions);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetPhysicalIdAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<string>(application.Id.ToString(CultureInfo.InvariantCulture));
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<ImmutableArray<string>>(application.PostLogoutRedirectUris);
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (application.Properties == null)
            {
                return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary.Create<string, JsonElement>());
            }

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(
                JsonSerializer.Deserialize<ImmutableDictionary<string, JsonElement>>(application.Properties.ToString()));
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<ImmutableArray<string>>(application.RedirectUris);
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableArray<string>> GetRequirementsAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<ImmutableArray<string>>(application.Requirements);
        }

        /// <inheritdoc/>
        public virtual ValueTask<TApplication> InstantiateAsync(CancellationToken cancellationToken)
            => new(new TApplication { ApplicationId = Guid.NewGuid().ToString("n") });

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TApplication> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = _session.Query<TApplication>(collection: OpenIdCollection);

            if (offset.HasValue)
            {
                query = query.Skip(offset.Value);
            }

            if (count.HasValue)
            {
                query = query.Take(count.Value);
            }

            return query.ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TResult> ListAsync<TState, TResult>(
            Func<IQueryable<TApplication>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public virtual ValueTask SetClientIdAsync(TApplication application,
            string identifier, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.ClientId = identifier;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetClientSecretAsync(TApplication application, string secret, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.ClientSecret = secret;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetClientTypeAsync(TApplication application, string type, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.Type = type;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetConsentTypeAsync(TApplication application, string type, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.ConsentType = type;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetDisplayNameAsync(TApplication application, string name, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.DisplayName = name;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetDisplayNamesAsync(TApplication application, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.DisplayNames = names;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetPermissionsAsync(TApplication application, ImmutableArray<string> permissions, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.Permissions = permissions;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetPostLogoutRedirectUrisAsync(TApplication application,
            ImmutableArray<string> uris, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.PostLogoutRedirectUris = uris;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetPropertiesAsync(TApplication application, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            if (properties == null || properties.IsEmpty)
            {
                application.Properties = null;

                return default;
            }

            application.Properties = JObject.Parse(JsonSerializer.Serialize(properties, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = false
            }));

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetRedirectUrisAsync(TApplication application,
            ImmutableArray<string> uris, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.RedirectUris = uris;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetRequirementsAsync(TApplication application,
            ImmutableArray<string> requirements, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.Requirements = requirements;

            return default;
        }

        /// <inheritdoc/>
        public virtual async ValueTask UpdateAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(application, checkConcurrency: true, collection: OpenIdCollection);

            try
            {
                await _session.SaveChangesAsync();
            }
            catch (ConcurrencyException exception)
            {
                throw new OpenIddictExceptions.ConcurrencyException(new StringBuilder()
                    .AppendLine("The application was concurrently updated and cannot be persisted in its current state.")
                    .Append("Reload the application from the database and retry the operation.")
                    .ToString(), exception);
            }
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableArray<string>> GetRolesAsync(TApplication application, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            return new ValueTask<ImmutableArray<string>>(application.Roles);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TApplication> ListInRoleAsync(string role, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(role))
            {
                throw new ArgumentException("The role name cannot be null or empty.", nameof(role));
            }

            return _session.Query<TApplication, OpenIdAppByRoleNameIndex>(index => index.RoleName == role, collection: OpenIdCollection).ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual ValueTask SetRolesAsync(TApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken)
        {
            if (application == null)
            {
                throw new ArgumentNullException(nameof(application));
            }

            application.Roles = roles;

            return default;
        }
    }
}
