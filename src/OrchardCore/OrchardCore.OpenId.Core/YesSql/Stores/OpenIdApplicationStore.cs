using System.Collections.Immutable;
using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OrchardCore.OpenId.Abstractions.Stores;
using OrchardCore.OpenId.YesSql.Indexes;
using OrchardCore.OpenId.YesSql.Models;
using YesSql;

namespace OrchardCore.OpenId.YesSql.Stores;

public class OpenIdApplicationStore<TApplication> : IOpenIdApplicationStore<TApplication>
    where TApplication : OpenIdApplication, new()
{
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = false
    };

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
        ArgumentNullException.ThrowIfNull(application);

        cancellationToken.ThrowIfCancellationRequested();

        await _session.SaveAsync(application, collection: OpenIdCollection);
        await _session.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask DeleteAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        cancellationToken.ThrowIfCancellationRequested();

        _session.Delete(application, collection: OpenIdCollection);
        await _session.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TApplication> FindByIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TApplication, OpenIdApplicationIndex>(index => index.ApplicationId == identifier, collection: OpenIdCollection).FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TApplication> FindByClientIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.Query<TApplication, OpenIdApplicationIndex>(index => index.ClientId == identifier, collection: OpenIdCollection).FirstOrDefaultAsync();
    }

    /// <inheritdoc/>
    public virtual async ValueTask<TApplication> FindByPhysicalIdAsync(string identifier, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(identifier);

        cancellationToken.ThrowIfCancellationRequested();

        return await _session.GetAsync<TApplication>(long.Parse(identifier, CultureInfo.InvariantCulture), collection: OpenIdCollection);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TApplication> FindByPostLogoutRedirectUriAsync(string uri, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(uri);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TApplication, OpenIdAppByLogoutUriIndex>(
            index => index.LogoutRedirectUri == uri,
            collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TApplication> FindByRedirectUriAsync(string uri, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(uri);

        cancellationToken.ThrowIfCancellationRequested();

        return _session.Query<TApplication, OpenIdAppByRedirectUriIndex>(
            index => index.RedirectUri == uri,
            collection: OpenIdCollection).ToAsyncEnumerable();
    }

    public virtual ValueTask<string> GetApplicationTypeAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.ApplicationType);
    }

    /// <inheritdoc/>
    public virtual ValueTask<TResult> GetAsync<TState, TResult>(
        Func<IQueryable<TApplication>, TState, IQueryable<TResult>> query,
        TState state, CancellationToken cancellationToken)
        => throw new NotSupportedException();

    /// <inheritdoc/>
    public virtual ValueTask<string> GetClientIdAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.ClientId);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetClientSecretAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.ClientSecret);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetClientTypeAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.Type);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetConsentTypeAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.ConsentType);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetDisplayNameAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.DisplayName);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<CultureInfo, string>> GetDisplayNamesAsync(
        TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (application.DisplayNames == null)
        {
            return ValueTask.FromResult(ImmutableDictionary.Create<CultureInfo, string>());
        }

        return ValueTask.FromResult(application.DisplayNames);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetIdAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.ApplicationId);
    }

    public virtual ValueTask<JsonWebKeySet> GetJsonWebKeySetAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (application.JsonWebKeySet is null)
        {
            return ValueTask.FromResult<JsonWebKeySet>(null);
        }

        return ValueTask.FromResult(JsonSerializer.Deserialize<JsonWebKeySet>(application.JsonWebKeySet.ToString(), JOptions.Default));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetPermissionsAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.Permissions);
    }

    /// <inheritdoc/>
    public virtual ValueTask<string> GetPhysicalIdAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.Id.ToString(CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetPostLogoutRedirectUrisAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.PostLogoutRedirectUris);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, JsonElement>> GetPropertiesAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (application.Properties == null)
        {
            return ValueTask.FromResult(ImmutableDictionary.Create<string, JsonElement>());
        }

        return ValueTask.FromResult(JConvert.DeserializeObject<ImmutableDictionary<string, JsonElement>>(application.Properties.ToString()));
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetRedirectUrisAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.RedirectUris);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableArray<string>> GetRequirementsAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.Requirements);
    }

    /// <inheritdoc/>
    public virtual ValueTask<ImmutableDictionary<string, string>> GetSettingsAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.Settings);
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
    public virtual ValueTask SetApplicationTypeAsync(TApplication application, string type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.ApplicationType = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetClientIdAsync(TApplication application,
        string identifier, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.ClientId = identifier;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetClientSecretAsync(TApplication application, string secret, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.ClientSecret = secret;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetClientTypeAsync(TApplication application, string type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.Type = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetConsentTypeAsync(TApplication application, string type, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.ConsentType = type;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDisplayNameAsync(TApplication application, string name, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.DisplayName = name;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetDisplayNamesAsync(TApplication application, ImmutableDictionary<CultureInfo, string> names, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.DisplayNames = names;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetJsonWebKeySetAsync(TApplication application, JsonWebKeySet set, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (set is not null)
        {
            application.JsonWebKeySet = JObject.Parse(JsonSerializer.Serialize(set, _serializerOptions));

            return default;
        }

        application.JsonWebKeySet = null;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPermissionsAsync(TApplication application, ImmutableArray<string> permissions, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.Permissions = permissions;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPostLogoutRedirectUrisAsync(TApplication application,
        ImmutableArray<string> uris, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.PostLogoutRedirectUris = uris;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetPropertiesAsync(TApplication application, ImmutableDictionary<string, JsonElement> properties, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (properties == null || properties.IsEmpty)
        {
            application.Properties = null;

            return default;
        }

        application.Properties = JObject.Parse(JConvert.SerializeObject(properties, JOptions.UnsafeRelaxedJsonEscaping));

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetRedirectUrisAsync(TApplication application,
        ImmutableArray<string> uris, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.RedirectUris = uris;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetRequirementsAsync(TApplication application,
        ImmutableArray<string> requirements, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.Requirements = requirements;

        return default;
    }

    /// <inheritdoc/>
    public virtual ValueTask SetSettingsAsync(TApplication application,
        ImmutableDictionary<string, string> settings, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.Settings = settings;

        return default;
    }

    /// <inheritdoc/>
    public virtual async ValueTask UpdateAsync(TApplication application, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        cancellationToken.ThrowIfCancellationRequested();

        await _session.SaveAsync(application, checkConcurrency: true, collection: OpenIdCollection);

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
        ArgumentNullException.ThrowIfNull(application);

        return ValueTask.FromResult(application.Roles);
    }

    /// <inheritdoc/>
    public virtual IAsyncEnumerable<TApplication> ListInRoleAsync(string role, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(role);

        return _session.Query<TApplication, OpenIdAppByRoleNameIndex>(index => index.RoleName == role, collection: OpenIdCollection).ToAsyncEnumerable();
    }

    /// <inheritdoc/>
    public virtual ValueTask SetRolesAsync(TApplication application, ImmutableArray<string> roles, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(application);

        application.Roles = roles;

        return default;
    }
}
