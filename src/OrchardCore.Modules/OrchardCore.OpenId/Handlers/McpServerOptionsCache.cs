using System.Collections.Concurrent;
using Microsoft.Extensions.Options;
using OpenIddict.Server;

namespace OrchardCore.OpenId.Handlers;

/// <summary>
/// Holds, per tenant URL, the server options that accept the MCP resource of that URL. Building them runs all
/// the options configurations of the server, which load its settings and keys, too much to do on every request.
/// A change to these settings releases the tenant, and this cache with it.
/// </summary>
public sealed class McpServerOptionsCache
{
    // A tenant without a host accepts any host name, so a caller must not be able to grow this cache.
    private const int MaximumEntries = 32;

    private readonly ConcurrentDictionary<string, OpenIddictServerOptions> _options = new(StringComparer.Ordinal);
    private readonly IOptionsFactory<OpenIddictServerOptions> _optionsFactory;

    /// <summary>Initializes the McpServerOptionsCache service.</summary>
    public McpServerOptionsCache(IOptionsFactory<OpenIddictServerOptions> optionsFactory)
    {
        _optionsFactory = optionsFactory;
    }

    /// <summary>Gets a separate options instance accepting the MCP resource of the specified tenant URL.</summary>
    /// <param name="baseUri">The base URI of the request.</param>
    /// <returns>The options to use for the request. They must not be changed.</returns>
    public OpenIddictServerOptions GetOptions(Uri baseUri)
    {
        var resource = McpClientRequestValidation.GetTenantEndpoint(baseUri, "mcp");
        if (_options.TryGetValue(resource, out var options))
        {
            return options;
        }

        options = _optionsFactory.Create(Options.DefaultName);
        options.Resources.Add(new Uri(resource));

        if (_options.Count >= MaximumEntries)
        {
            _options.Clear();
        }

        return _options.GetOrAdd(resource, options);
    }
}
