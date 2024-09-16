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

namespace OrchardCore.OpenId.YesSql.Stores;

public class OpenIdScopeStore<TScope> : IOpenIdScopeStore<TScope>
    where TScope : OpenIdScope, new()
{
    private const string OpenIdCollection = OpenIdScope.OpenIdCollection;
    private readonly ISession _session;

    public OpenIdScopeStore(ISession session)
    {
        _session = session;
    }

    /// <inheritdoc/>
    public virtual async ValueTask<long> CountAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TScope>(collection: OpenIdCollection).CountAsync();
    }

    /// <inheritdoc/>
    public virtual ValueTask<long> CountAsync<TResult>(Func<IQueryable<TScope>, IQueryable<TResult>> query, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public virtual async ValueTask CreateAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        cancellationToken.ThrowIfCancellationRequested();

        await _session.SaveAsync(scope, collection: OpenIdCollection);
        await _session.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask DeleteAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        cancellationToken.ThrowIfCancellationRequested();

        _session.Delete(scope, collection: OpenIdCollection);
        await _session.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TScope> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TScope, OpenIdScopeIndex>(index => index.ScopeId == identifier, collection: OpenIdCollection).FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TScope> FindByNameAsync(string name, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TScope, OpenIdScopeIndex>(index => index.Name == name, collection: OpenIdCollection).FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TScope> FindByNamesAsync(
        ImmutableArray<string> names, CancellationToken cancellationToken)
    {
        if (names.Any(name => string.IsNullOrEmpty(name)))
        {
            throw new ArgumentException("Scope names cannot be null or empty.", nameof(names));
        }

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TScope, OpenIdScopeIndex>(index => index.Name.IsIn(names), collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TScope> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.GetAsync<TScope>(long.Parse(identifier, CultureInfo.InvariantCulture), collection: OpenIdCollection);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TScope> FindByResourceAsync(string resource, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(resource);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TScope, OpenIdScopeByResourceIndex>(
            index => index.Resource == resource,
            collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual ValueTask<TResult> GetAsync<TState, TResult>(
        Func<IQueryable<TScope>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public virtual ValueTask<string> GetDescriptionAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return ValueTask.FromResult(scope.Description);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDescriptionsAsync(
        TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (scope.Descriptions == null)
        {
            return ValueTask.FromResult(ImmutableDictionary.Create<CultureInfo, string>());
        }

        return ValueTask.FromResult(scope.Descriptions);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetDisplayNameAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return ValueTask.FromResult(scope.DisplayName);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(
        TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (scope.DisplayNames == null)
        {
            return ValueTask.FromResult(ImmutableDictionary.Create<CultureInfo, string>());
        }

        return ValueTask.FromResult(scope.DisplayNames);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetIdAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return ValueTask.FromResult(scope.ScopeId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetNameAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return ValueTask.FromResult(scope.Name);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetPhysicalIdAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return ValueTask.FromResult(scope.Id.ToString(CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (scope.Properties == null)
        {
            return ValueTask.FromResult<ImmutableDictionary<string, JsonElement>>(ImmutableDictionary.Create<string, JsonElement>());
        }

        return ValueTask.FromResult(JConvert.DeserializeObject<ImmutableDictionary<string, JsonElement>>(scope.Properties.ToString()));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetResourcesAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        return ValueTask.FromResult(scope.Resources);
    }

    /// <inheritdoc/>
    public virtual ValueTask<TScope> InstantiateAsync(CancellationToken cancellationToken)
        => new(new TScope { ScopeId = Guid.NewGuid().ToString("n") });

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TScope> ListAsync(int? count, int? offset, CancellationToken cancellationToken)
    {
        var query = _session.Query<TScope>(collection: OpenIdCollection);

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
        Func<IQueryable<TScope>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public virtual ValueTask SetDescriptionAsync(TScope scope, string description, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        scope.Description = description;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDescriptionsAsync(TScope scope,
        ImmutableDictionary<CultureInfo, string> descriptions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        scope.Descriptions = descriptions;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDisplayNameAsync(TScope scope, string name, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        scope.DisplayName = name;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDisplayNamesAsync(TScope scope,
        ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        scope.DisplayNames = names;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetNameAsync(TScope scope, string name, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        scope.Name = name;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPropertiesAsync(TScope scope, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        if (properties == null || properties.IsEmpty)
        {
            scope.Properties = null;

            return default;
        }

        scope.Properties = JObject.Parse(JConvert.SerializeObject(properties, JOptions.UnsafeRelaxedJsonEscaping));

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetResourcesAsync(TScope scope, ImmutableArray<string> resources, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        scope.Resources = resources;

        return default;
    }

    /// <inheritdoc/>
    public virtual async ValueTask UpdateAsync(TScope scope, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(scope);

        cancellationToken.ThrowIfCancellationRequested();

        await _session.SaveAsync(scope, checkConcurrency: true, collection: OpenIdCollection);

        try
        {
            await _session.SaveChangesAsync();
        }
        catch (ConcurrencyException exception)
        {
            throw new OpenIddictExceptions.ConcurrencyException(new StringBuilder()
                .AppendLine("The scope was concurrently updated and cannot be persisted in its current state.")
                .Append("Reload the scope from the database and retry the operation.")
                .ToString(), exception);
        }
    }
}
