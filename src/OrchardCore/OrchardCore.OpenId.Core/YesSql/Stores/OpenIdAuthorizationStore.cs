using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
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
using YesSql.Services;

namespace OrchardCore.OpenId.YesSql.Stores
{
    public class OpenIdAuthorizationStore<TAuthorization> : IOpenIdAuthorizationStore<TAuthorization>
        where TAuthorization : OpenIdAuthorization, new()
    {
        private readonly ISession _session;
        private const string OpenIdCollection = OpenIdAuthorization.OpenIdCollection;

        public OpenIdAuthorizationStore(ISession session)
        {
            _session = session;
        }

        /// <inheritdoc/>
        public virtual async ValueTask<long> CountAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<TAuthorization>(collection: OpenIdCollection).CountAsync();
        }

        /// <inheritdoc/>
        public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<TAuthorization>, IQueryable<TResult>> query, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public virtual async ValueTask CreateAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(authorization, collection: OpenIdCollection);
            await _session.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual async ValueTask DeleteAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Delete(authorization, collection: OpenIdCollection);
            await _session.SaveChangesAsync();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TAuthorization> FindAsync(
            string subject, string client, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (String.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client cannot be null or empty.", nameof(client));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
                index => index.ApplicationId == client && index.Subject == subject,
                collection: OpenIdCollection).ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TAuthorization> FindAsync(
            string subject, string client, string status, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (String.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (String.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(client));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
                index => index.ApplicationId == client && index.Subject == subject && index.Status == status,
                collection: OpenIdCollection).ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TAuthorization> FindAsync(
            string subject, string client,
            string status, string type, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            if (String.IsNullOrEmpty(client))
            {
                throw new ArgumentException("The client identifier cannot be null or empty.", nameof(client));
            }

            if (String.IsNullOrEmpty(status))
            {
                throw new ArgumentException("The status cannot be null or empty.", nameof(client));
            }

            if (String.IsNullOrEmpty(type))
            {
                throw new ArgumentException("The type cannot be null or empty.", nameof(client));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
                index => index.ApplicationId == client && index.Subject == subject &&
                         index.Status == status && index.Type == type,
                collection: OpenIdCollection).ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<TAuthorization> FindAsync(
            string subject, string client, string status, string type,
            ImmutableArray<string> scopes, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await foreach (var authorization in FindAsync(subject, client, status, type, cancellationToken))
            {
                if (new HashSet<string>(await GetScopesAsync(authorization, cancellationToken), StringComparer.Ordinal).IsSupersetOf(scopes))
                {
                    yield return authorization;
                }
            }
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TAuthorization> FindByApplicationIdAsync(
            string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
                index => index.ApplicationId == identifier,
                collection: OpenIdCollection).ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual async ValueTask<TAuthorization> FindByIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
                index => index.AuthorizationId == identifier,
                collection: OpenIdCollection).FirstOrDefaultAsync();
        }

        /// <inheritdoc/>
        public virtual async ValueTask<TAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(identifier))
            {
                throw new ArgumentException("The identifier cannot be null or empty.", nameof(identifier));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return await _session.GetAsync<TAuthorization>(Int64.Parse(identifier, CultureInfo.InvariantCulture), collection: OpenIdCollection);
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TAuthorization> FindBySubjectAsync(
            string subject, CancellationToken cancellationToken)
        {
            if (String.IsNullOrEmpty(subject))
            {
                throw new ArgumentException("The subject cannot be null or empty.", nameof(subject));
            }

            cancellationToken.ThrowIfCancellationRequested();

            return _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
                index => index.Subject == subject,
                collection: OpenIdCollection).ToAsyncEnumerable();
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetApplicationIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string>(authorization.ApplicationId);
        }

        /// <inheritdoc/>
        public virtual ValueTask<TResult> GetAsync<TState, TResult>(
            Func<IQueryable<TAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public virtual ValueTask<DateTimeOffset?> GetCreationDateAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (authorization.CreationDate is null)
            {
                return new ValueTask<DateTimeOffset?>(result: null);
            }

            return new ValueTask<DateTimeOffset?>(DateTime.SpecifyKind(authorization.CreationDate.Value, DateTimeKind.Utc));
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string>(authorization.AuthorizationId);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetPhysicalIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string>(authorization.Id.ToString(CultureInfo.InvariantCulture));
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (authorization.Properties == null)
            {
                return new ValueTask<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary.Create<string, JsonElement>());
            }

            return new ValueTask<ImmutableDictionary<string, JsonElement>>(
                JsonSerializer.Deserialize<ImmutableDictionary<string, JsonElement>>(authorization.Properties.ToString()));
        }

        /// <inheritdoc/>
        public virtual ValueTask<ImmutableArray<string>> GetScopesAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<ImmutableArray<string>>(authorization.Scopes);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetStatusAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string>(authorization.Status);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetSubjectAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string>(authorization.Subject);
        }

        /// <inheritdoc/>
        public virtual ValueTask<string> GetTypeAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            return new ValueTask<string>(authorization.Type);
        }

        /// <inheritdoc/>
        public virtual ValueTask<TAuthorization> InstantiateAsync(CancellationToken cancellationToken)
            => new(new TAuthorization { AuthorizationId = Guid.NewGuid().ToString("n") });

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TAuthorization> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
        {
            var query = _session.Query<TAuthorization>(collection: OpenIdCollection);

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
            Func<IQueryable<TAuthorization>, TState, IQueryable<TResult>> query,
            TState state, CancellationToken cancellationToken)
            => throw new NotSupportedException();

        /// <inheritdoc/>
        public virtual async ValueTask PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
        {
            // Note: YesSql doesn't support set-based deletes, which prevents removing entities
            // in a single command without having to retrieve and materialize them first.
            // To work around this limitation, entities are manually listed and deleted using a batch logic.

            IList<Exception> exceptions = null;

            for (var i = 0; i < 1000; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var authorizations = await _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
                    authorization => authorization.CreationDate < threshold.UtcDateTime &&
                                    (authorization.Status != OpenIddictConstants.Statuses.Valid ||
                                    (authorization.Type == OpenIddictConstants.AuthorizationTypes.AdHoc &&
                                     authorization.AuthorizationId.IsNotIn<OpenIdTokenIndex>(
                                         token => token.AuthorizationId,
                                         token => token.Id != 0))),
                    collection: OpenIdCollection).Take(100).ListAsync();

                if (!authorizations.Any())
                {
                    return;
                }

                foreach (var authorization in authorizations)
                {
                    _session.Delete(authorization, collection: OpenIdCollection);
                }

                try
                {
                    await _session.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    exceptions ??= new List<Exception>(capacity: 1);

                    exceptions.Add(exception);
                }
            }

            if (exceptions != null)
            {
                throw new AggregateException("An error occurred while pruning authorizations.", exceptions);
            }
        }

        /// <inheritdoc/>
        public virtual ValueTask SetApplicationIdAsync(TAuthorization authorization,
            string identifier, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (String.IsNullOrEmpty(identifier))
            {
                authorization.ApplicationId = null;
            }
            else
            {
                authorization.ApplicationId = identifier;
            }

            return default;
        }

        public ValueTask SetCreationDateAsync(TAuthorization authorization, DateTimeOffset? date, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.CreationDate = date?.UtcDateTime;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetPropertiesAsync(TAuthorization authorization, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            if (properties == null || properties.IsEmpty)
            {
                authorization.Properties = null;

                return default;
            }

            authorization.Properties = JObject.Parse(JsonSerializer.Serialize(properties, new JsonSerializerOptions
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                WriteIndented = false
            }));

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetScopesAsync(TAuthorization authorization,
            ImmutableArray<string> scopes, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Scopes = scopes;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetStatusAsync(TAuthorization authorization,
            string status, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Status = status;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetSubjectAsync(TAuthorization authorization,
            string subject, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Subject = subject;

            return default;
        }

        /// <inheritdoc/>
        public virtual ValueTask SetTypeAsync(TAuthorization authorization,
            string type, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            authorization.Type = type;

            return default;
        }

        /// <inheritdoc/>
        public virtual async ValueTask UpdateAsync(TAuthorization authorization, CancellationToken cancellationToken)
        {
            if (authorization == null)
            {
                throw new ArgumentNullException(nameof(authorization));
            }

            cancellationToken.ThrowIfCancellationRequested();

            _session.Save(authorization, checkConcurrency: true, collection: OpenIdCollection);

            try
            {
                await _session.SaveChangesAsync();
            }
            catch (ConcurrencyException exception)
            {
                throw new OpenIddictExceptions.ConcurrencyException(new StringBuilder()
                    .AppendLine("The authorization was concurrently updated and cannot be persisted in its current state.")
                    .Append("Reload the authorization from the database and retry the operation.")
                    .ToString(), exception);
            }
        }
    }
}
