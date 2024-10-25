using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using YesSql;
using YesSql.Services;
using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OrchardCore.OpenId.YesSql.Stores;

public class OpenIdTokenStore<TToken> : IOpenIdTokenStore<TToken>
    where TToken : OpenIdToken, new()
{
    private readonly ISession _session;
    private const string OpenIdCollection = OpenIdToken.OpenIdCollection;

    public OpenIdTokenStore(ISession session)
    {
        _session = session;
    }

    /// <inheritdoc/>
    public virtual async ValueTask<long> CountAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TToken>(collection: OpenIdCollection).CountAsync();
    }

    /// <inheritdoc/>
    public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<TToken>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public virtual async ValueTask CreateAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        cancellationToken.ThrowIfCancellationRequested();

        await _session.SaveAsync(token, collection: OpenIdCollection);
        await _session.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask DeleteAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        cancellationToken.ThrowIfCancellationRequested();

        _session.Delete(token, collection: OpenIdCollection);
        await _session.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindAsync(
        string subject, string client, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(client);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TToken, OpenIdTokenIndex>(
            index => index.ApplicationId == client && index.Subject == subject, collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindAsync(
        string subject, string client, string status, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(client);
        ArgumentException.ThrowIfNullOrEmpty(status);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TToken, OpenIdTokenIndex>(
            index => index.ApplicationId == client && index.Subject == subject && index.Status == status, collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindAsync(
        string subject, string client, string status, string type, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);
        ArgumentException.ThrowIfNullOrEmpty(client);
        ArgumentException.ThrowIfNullOrEmpty(status);
        ArgumentException.ThrowIfNullOrEmpty(type);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TToken, OpenIdTokenIndex>(
            index => index.ApplicationId == client && index.Subject == subject &&
                     index.Status == status && index.Type == type, collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindByApplicationIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TToken, OpenIdTokenIndex>(index => index.ApplicationId == identifier, collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TToken, OpenIdTokenIndex>(index => index.AuthorizationId == identifier, collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TToken> FindByReferenceIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TToken, OpenIdTokenIndex>(index => index.ReferenceId == identifier, collection: OpenIdCollection).FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TToken> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TToken, OpenIdTokenIndex>(index => index.TokenId == identifier, collection: OpenIdCollection).FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TToken> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.GetAsync<TToken>(long.Parse(identifier, CultureInfo.InvariantCulture), collection: OpenIdCollection);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> FindBySubjectAsync(string subject, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(subject);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TToken, OpenIdTokenIndex>(index => index.Subject == subject, collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual ValueTask<TResult> GetAsync<TState, TResult>(
        Func<IQueryable<TToken>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public virtual ValueTask<string> GetApplicationIdAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.ApplicationId?.ToString(CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetAuthorizationIdAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.AuthorizationId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<DateTimeOffset?> GetCreationDateAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.CreationDate is null)
        {
            return ValueTask.FromResult<DateTimeOffset?>(result: null);
        }

        return ValueTask.FromResult<DateTimeOffset?>(DateTime.SpecifyKind(token.CreationDate.Value, DateTimeKind.Utc));
    }

    /// <inheritdoc/>
    public virtual ValueTask<DateTimeOffset?> GetExpirationDateAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.ExpirationDate is null)
        {
            return ValueTask.FromResult<DateTimeOffset?>(result: null);
        }

        return ValueTask.FromResult<DateTimeOffset?>(DateTime.SpecifyKind(token.ExpirationDate.Value, DateTimeKind.Utc));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetIdAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.TokenId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetPayloadAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.Payload);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetPhysicalIdAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.Id.ToString(CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.Properties == null)
        {
            return ValueTask.FromResult(ImmutableDictionary.Create<string, JsonElement>());
        }

        return ValueTask.FromResult(JConvert.DeserializeObject<ImmutableDictionary<string, JsonElement>>(token.Properties.ToString()));
    }

    /// <inheritdoc/>
    public virtual ValueTask<DateTimeOffset?> GetRedemptionDateAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (token.RedemptionDate is null)
        {
            return ValueTask.FromResult<DateTimeOffset?>(result: null);
        }

        return ValueTask.FromResult<DateTimeOffset?>(DateTime.SpecifyKind(token.RedemptionDate.Value, DateTimeKind.Utc));
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetReferenceIdAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.ReferenceId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetStatusAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.Status);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetSubjectAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.Subject);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetTypeAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        return ValueTask.FromResult(token.Type);
    }

    /// <inheritdoc/>
    public virtual ValueTask<TToken> InstantiateAsync(CancellationToken cancellationToken)
        => new(new TToken { TokenId = Guid.NewGuid().ToString("n") });

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TToken> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        var query = _session.Query<TToken>(collection: OpenIdCollection);

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
        Func<IQueryable<TToken>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public virtual async ValueTask<long> PruneAsync(DateTimeOffset threshold, CancellationToken cancellationToken = default)
    {
        // Note: YesSql doesn't support set-based deletes, which prevents removing entities
        // in a single command without having to retrieve and materialize them first.
        // To work around this limitation, entities are manually listed and deleted using a batch logic.

        List<Exception> exceptions = null;

        var result = 0L;

        for (var i = 0; i < 1000; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var tokens = (await _session.Query<TToken, OpenIdTokenIndex>(
                token => token.CreationDate < threshold.UtcDateTime &&
                       ((token.Status != Statuses.Inactive && token.Status != Statuses.Valid) ||
                         token.AuthorizationId.IsNotIn<OpenIdAuthorizationIndex>(
                            authorization => authorization.AuthorizationId,
                            authorization => authorization.Status == Statuses.Valid) ||
                         token.ExpirationDate < DateTime.UtcNow), collection: OpenIdCollection).Take(100).ListAsync()).ToList();

            if (tokens.Count is 0)
            {
                break;
            }

            foreach (var token in tokens)
            {
                _session.Delete(token, collection: OpenIdCollection);
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

            result += tokens.Count;
        }

        if (exceptions != null)
        {
            throw new AggregateException("An error occurred while pruning authorizations.", exceptions);
        }

        return result;
    }

    /// <inheritdoc/>
    public virtual async ValueTask<long> RevokeByAuthorizationIdAsync(string identifier, CancellationToken cancellationToken)
    {
        // Note: YesSql doesn't support set-based updates, which prevents updating entities
        // in a single command without having to retrieve and materialize them first.
        // To work around this limitation, entities are manually listed and updated.

        cancellationToken.ThrowIfCancellationRequested();

        var tokens = (await _session.Query<TToken, OpenIdTokenIndex>(
            token => token.AuthorizationId == identifier, collection: OpenIdCollection).ListAsync()).ToList();

        if (tokens.Count is 0)
        {
            return 0;
        }

        foreach (var token in tokens)
        {
            token.Status = Statuses.Revoked;

            await _session.SaveAsync(token, checkConcurrency: false, collection: OpenIdCollection);
        }

        await _session.SaveChangesAsync();

        return tokens.Count;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetApplicationIdAsync(TToken token, string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (string.IsNullOrEmpty(identifier))
        {
            token.ApplicationId = null;
        }
        else
        {
            token.ApplicationId = identifier;
        }

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetAuthorizationIdAsync(TToken token, string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (string.IsNullOrEmpty(identifier))
        {
            token.AuthorizationId = null;
        }
        else
        {
            token.AuthorizationId = identifier;
        }

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetCreationDateAsync(TToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.CreationDate = date?.UtcDateTime;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetExpirationDateAsync(TToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.ExpirationDate = date?.UtcDateTime;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPayloadAsync(TToken token, string payload, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.Payload = payload;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPropertiesAsync(TToken token, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (properties == null || properties.IsEmpty)
        {
            token.Properties = null;

            return default;
        }

        token.Properties = JObject.Parse(JConvert.SerializeObject(properties, JOptions.UnsafeRelaxedJsonEscaping));

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetRedemptionDateAsync(TToken token, DateTimeOffset? date, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.RedemptionDate = date?.UtcDateTime;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetReferenceIdAsync(TToken token, string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.ReferenceId = identifier;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetStatusAsync(TToken token, string status, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.Status = status;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetSubjectAsync(TToken token, string subject, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.Subject = subject;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetTypeAsync(TToken token, string type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        token.Type = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual async ValueTask UpdateAsync(TToken token, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(token);

        cancellationToken.ThrowIfCancellationRequested();

        await _session.SaveAsync(token, checkConcurrency: true, collection: OpenIdCollection);

        try
        {
            await _session.SaveChangesAsync();
        }
        catch (ConcurrencyException exception)
        {
            throw new OpenIddictExceptions.ConcurrencyException(new StringBuilder()
                .AppendLine("The token was concurrently updated and cannot be persisted in its current state.")
                .Append("Reload the token from the database and retry the operation.")
                .ToString(), exception);
        }
    }
}
