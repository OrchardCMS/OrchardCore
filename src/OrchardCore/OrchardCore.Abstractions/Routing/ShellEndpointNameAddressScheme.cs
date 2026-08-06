using System.Collections.Frozen;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;

namespace OrchardCore.Routing;

/// <summary>
/// A tenant aware <see cref="IEndpointAddressScheme{TAddress}"/> that resolves endpoints by their
/// endpoint name for link generation (<c>LinkGenerator.GetPathByName()</c> / <c>GetUriByName()</c>).
/// </summary>
/// <remarks>
/// This replaces the framework's default endpoint name address scheme, which throws an
/// <see cref="InvalidOperationException"/> as soon as two link generating endpoints share the same
/// endpoint name. A tenant that maps a dynamic controller route (as <c>OrchardCore.Autoroute</c>,
/// <c>OrchardCore.HomeRoute</c> and <c>OrchardCore.Sitemaps</c> do) makes the shared controller
/// endpoint data source also emit a second, non-routable placeholder endpoint for every action,
/// copying the action metadata including its <c>[EndpointName]</c>. For an attribute-routed action
/// that placeholder therefore shares the endpoint name of the routable one, which used to make every
/// link generation by name throw.
/// <para>
/// The non-routable placeholders (which cannot generate a URL) are ignored when a name is also owned
/// by a real <see cref="RouteEndpoint"/>, so link generation by name keeps working. Genuine
/// misconfiguration is still surfaced: if the same name is owned by more than one
/// <see cref="RouteEndpoint"/> the scheme throws, exactly like the framework, because such a link
/// would otherwise resolve to an arbitrary endpoint depending on registration order.
/// </para>
/// </remarks>
public sealed class ShellEndpointNameAddressScheme : IEndpointAddressScheme<string>
{
    private readonly EndpointDataSource _dataSource;
    private readonly object _lock = new();

    private CacheEntry _cache;

    public ShellEndpointNameAddressScheme(EndpointDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    public IEnumerable<Endpoint> FindEndpoints(string address)
    {
        ArgumentNullException.ThrowIfNull(address);

        var lookup = EnsureLookup();

        return lookup.TryGetValue(address, out var endpoints) ? endpoints : [];
    }

    private FrozenDictionary<string, Endpoint[]> EnsureLookup()
    {
        var cache = Volatile.Read(ref _cache);

        if (cache is not null && !cache.ChangeToken.HasChanged)
        {
            return cache.Lookup;
        }

        lock (_lock)
        {
            cache = _cache;

            if (cache is not null && !cache.ChangeToken.HasChanged)
            {
                return cache.Lookup;
            }

            // Capture the change token before reading the endpoints so that a change occurring
            // during the build invalidates the lookup on the next access. The token and the lookup
            // are published together as a single immutable entry so that a reader never observes a
            // new token paired with a stale lookup.
            var changeToken = _dataSource.GetChangeToken();
            var lookup = BuildLookup(_dataSource.Endpoints);

            Volatile.Write(ref _cache, new CacheEntry(changeToken, lookup));

            return lookup;
        }
    }

    private static FrozenDictionary<string, Endpoint[]> BuildLookup(IReadOnlyList<Endpoint> endpoints)
    {
        var entries = new Dictionary<string, List<Endpoint>>(StringComparer.Ordinal);

        for (var i = 0; i < endpoints.Count; i++)
        {
            var endpoint = endpoints[i];

            if (endpoint.Metadata.GetMetadata<ISuppressLinkGenerationMetadata>()?.SuppressLinkGeneration == true)
            {
                // Skip anything that is suppressed for linking, mirroring the framework scheme.
                continue;
            }

            var endpointName = endpoint.Metadata.GetMetadata<IEndpointNameMetadata>()?.EndpointName;

            if (endpointName is null)
            {
                continue;
            }

            if (!entries.TryGetValue(endpointName, out var group))
            {
                group = [];
                entries[endpointName] = group;
            }

            group.Add(endpoint);
        }

        ThrowIfAmbiguous(entries);

        return entries.ToFrozenDictionary(entry => entry.Key, entry => entry.Value.ToArray(), StringComparer.Ordinal);
    }

    private static void ThrowIfAmbiguous(Dictionary<string, List<Endpoint>> entries)
    {
        StringBuilder builder = null;

        foreach (var (name, group) in entries)
        {
            // A name may be owned by a single real endpoint plus one or more non-routable
            // placeholders emitted for dynamic controller routes; that is expected. It is only
            // ambiguous when more than one endpoint that can actually generate a URL shares a name.
            var routableCount = 0;

            for (var i = 0; i < group.Count; i++)
            {
                if (group[i] is RouteEndpoint)
                {
                    routableCount++;
                }
            }

            if (routableCount <= 1)
            {
                continue;
            }

            builder ??= new StringBuilder().AppendLine("The following endpoints with a duplicate endpoint name were found.");

            builder.AppendLine().Append("Endpoints with endpoint name '").Append(name).AppendLine("':");

            for (var i = 0; i < group.Count; i++)
            {
                if (group[i] is RouteEndpoint)
                {
                    builder.AppendLine(group[i].DisplayName);
                }
            }
        }

        if (builder is not null)
        {
            throw new InvalidOperationException(builder.ToString());
        }
    }

    private sealed record CacheEntry(IChangeToken ChangeToken, FrozenDictionary<string, Endpoint[]> Lookup);
}
