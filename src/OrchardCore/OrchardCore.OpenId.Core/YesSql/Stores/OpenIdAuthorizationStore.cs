using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using YesSql;
using YesSql.Services;

namespace OrchardCore.OpenId.YesSql.Stores;

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
        ArgumentNullException.ThrowIfNull(authorization);

        cancellationToken.ThrowIfCancellationRequested();

        await _session.SaveAsync(authorization, collection: OpenIdCollection);
        await _session.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask DeleteAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        cancellationToken.ThrowIfCancellationRequested();

        _session.Delete(authorization, collection: OpenIdCollection);
        await _session.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindAsync(
        string subject, string client, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(client);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
            index => index.ApplicationId == client && index.Subject == subject,
            collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindAsync(
        string subject, string client, string status, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(client);
        ArgumentException.ThrowIfNullOrEmpty(status);

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
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(client);
        ArgumentException.ThrowIfNullOrEmpty(status);
        ArgumentException.ThrowIfNullOrEmpty(type);

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
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
            index => index.ApplicationId == identifier,
            collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TAuthorization> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
            index => index.AuthorizationId == identifier,
            collection: OpenIdCollection).FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TAuthorization> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.GetAsync<TAuthorization>(long.Parse(identifier, CultureInfo.InvariantCulture), collection: OpenIdCollection);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TAuthorization> FindBySubjectAsync(
        string subject, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
            index => index.Subject == subject,
            collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetApplicationIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        return ValueTask.FromResult(authorization.ApplicationId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<TResult> GetAsync<TState, TResult>(
        Func<IQueryable<TAuthorization>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public virtual ValueTask<DateTimeOffset?> GetCreationDateAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        if (authorization.CreationDate is null)
        {
            return ValueTask.FromResult<DateTimeOffset?>(result: null);
        }

        return ValueTask.FromResult<DateTimeOffset?>(DateTime.SpecifyKind(authorization.CreationDate.Value, DateTimeKind.Utc));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        return ValueTask.FromResult(authorization.AuthorizationId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetPhysicalIdAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        return ValueTask.FromResult(authorization.Id.ToString(CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        if (authorization.Properties == null)
        {
            return ValueTask.FromResult(ImmutableDictionary.Create<string, JsonElement>());
        }

        return ValueTask.FromResult(JConvert.DeserializeObject<ImmutableDictionary<string, JsonElement>>(authorization.Properties.ToString()));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetScopesAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        return ValueTask.FromResult(authorization.Scopes);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetStatusAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        return ValueTask.FromResult(authorization.Status);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetSubjectAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        return ValueTask.FromResult(authorization.Subject);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetTypeAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        return ValueTask.FromResult(authorization.Type);
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
    public virtual async ValueTask<long> PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken)
    {
        // Note: YesSql doesn't support set-based deletes, which prevents removing entities
        // in a single command without having to retrieve and materialize them first.
        // To work around this limitation, entities are manually listed and deleted using a batch logic.

        List<Exception> exceptions = null;

        var result = 0L;

        for (var i = 0; i < 1000; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var authorizations = (await _session.Query<TAuthorization, OpenIdAuthorizationIndex>(
                authorization => authorization.CreationDate < threshold.UtcDateTime &&
                                (authorization.Status != OpenIddictConstants.Statuses.Valid ||
                                (authorization.Type == OpenIddictConstants.AuthorizationTypes.AdHoc &&
                                 authorization.AuthorizationId.IsNotIn<OpenIdTokenIndex>(
                                     token => token.AuthorizationId,
                                     token => token.Id != 0))),
                collection: OpenIdCollection).Take(100).ListAsync()).ToList();

            if (authorizations.Count is 0)
            {
                break;
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

                continue;
            }

            result += authorizations.Count;
        }

        if (exceptions != null)
        {
            throw new AggregateException("An error occurred while pruning authorizations.", exceptions);
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetApplicationIdAsync(TAuthorization authorization,
        string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        if (string.IsNullOrEmpty(identifier))
        {
            authorization.ApplicationId = null;
        }
        else
        {
            authorization.ApplicationId = identifier;
        }

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetCreationDateAsync(TAuthorization authorization, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        authorization.CreationDate = date?.UtcDateTime;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPropertiesAsync(TAuthorization authorization, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        if (properties == null || properties.IsEmpty)
        {
            authorization.Properties = null;

            return default;
        }

        authorization.Properties = JObject.Parse(JConvert.SerializeObject(properties, JOptions.UnsafeRelaxedJsonEscaping));

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetScopesAsync(TAuthorization authorization,
        ImmutableArray<string> scopes, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        authorization.Scopes = scopes;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetStatusAsync(TAuthorization authorization,
        string status, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        authorization.Status = status;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetSubjectAsync(TAuthorization authorization,
        string subject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        authorization.Subject = subject;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetTypeAsync(TAuthorization authorization,
        string type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        authorization.Type = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual async ValueTask UpdateAsync(TAuthorization authorization, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorization);

        cancellationToken.ThrowIfCancellationRequested();

        await _session.SaveAsync(authorization, checkConcurrency: true, collection: OpenIdCollection);

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
