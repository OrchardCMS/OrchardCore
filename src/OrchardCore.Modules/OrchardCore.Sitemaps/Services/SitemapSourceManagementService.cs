using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using OrchardCore.Sitemaps.Models;

namespace OrchardCore.Sitemaps.Services;

internal sealed class SitemapSourceManagementService
{
    internal static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        TypeInfoResolver = new System.Text.Json.Serialization.Metadata.DefaultJsonTypeInfoResolver(),
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,
        Converters = { new JsonStringEnumConverter(allowIntegerValues: false) },
    };
    private readonly ISitemapManager _manager;
    private readonly ISitemapIdGenerator _ids;
    private readonly IEnumerable<ISitemapSourceFactory> _factories;
    private readonly IRouteableContentTypeCoordinator _definitions;

    public SitemapSourceManagementService(ISitemapManager manager, ISitemapIdGenerator ids,
        IEnumerable<ISitemapSourceFactory> factories, IRouteableContentTypeCoordinator definitions)
    {
        _manager = manager;
        _ids = ids;
        _factories = factories;
        _definitions = definitions;
    }

    public string[] Types() => _factories.Select(factory => factory.Name)
        .Where(name => name is nameof(CustomPathSitemapSource) or nameof(ContentTypesSitemapSource)).Order(StringComparer.Ordinal).ToArray();

    public async Task<SitemapSourceMutationResult> SaveAsync(string sitemapId, SitemapSourceDefinition input, string sourceId = null)
    {
        var sitemap = await _manager.LoadSitemapAsync(sitemapId);
        if (sitemap is null) { return new() { NotFound = true }; }
        if (sitemap is not Sitemap) { return Error("sitemap", "Sources can only be managed on regular sitemaps; update an index's contained sitemap IDs instead."); }
        var existing = sourceId is null ? null : sitemap.SitemapSources.FirstOrDefault(source => source.Id == sourceId);
        if (sourceId is not null && existing is null) { return new() { NotFound = true }; }
        if (input?.Configuration is null || !Types().Contains(input.Type)) { return Error("type", "Select a registered source type with a complete configuration."); }
        if (existing is not null && existing.GetType().Name != input.Type) { return Error("type", "An existing source's type cannot change."); }
        if (input.Configuration.Any(entry => entry.Key.Equals("id", StringComparison.OrdinalIgnoreCase)
            || entry.Key.Equals("lastUpdate", StringComparison.OrdinalIgnoreCase))) { return Error("configuration", "Source IDs and update timestamps are server controlled."); }
        SitemapSource proposed;
        try
        {
            proposed = input.Type == nameof(CustomPathSitemapSource)
                ? input.Configuration.Deserialize<CustomPathSitemapSource>(JsonOptions)
                : input.Configuration.Deserialize<ContentTypesSitemapSource>(JsonOptions);
        }
        catch (JsonException) { return Error("configuration", "The source configuration contains unknown properties or invalid value types."); }
        var errors = SitemapSourceValidation.Validate(proposed);
        if (proposed is ContentTypesSitemapSource content && !content.IndexAll)
        {
            var names = content.LimitItems ? new[] { content.LimitedContentType?.ContentTypeName }
                : content.ContentTypes?.Select(entry => entry?.ContentTypeName) ?? [];
            var available = (await _definitions.ListRoutableTypeDefinitionsAsync()).Select(definition => definition.Name).ToHashSet(StringComparer.Ordinal);
            foreach (var name in names)
            {
                if (string.IsNullOrWhiteSpace(name) || !available.Contains(name))
                {
                    errors["contentTypes"] = ["Select existing routable content types."];
                    break;
                }
            }
        }
        if (errors.Count > 0) { return new() { Errors = errors }; }
        if (existing is not null && JsonNode.DeepEquals(Describe(existing).Configuration, Describe(proposed).Configuration))
        {
            return new() { Source = existing };
        }
        proposed.Id = existing?.Id ?? _ids.GenerateUniqueId();
        if (existing is not null) { sitemap.SitemapSources.Remove(existing); }
        sitemap.SitemapSources.Add(proposed);
        await _manager.UpdateSitemapAsync(sitemap);
        return new() { Source = proposed, Changed = true };
    }

    internal static SitemapSourceResponse Describe(SitemapSource source)
    {
        JsonObject configuration = source switch
        {
            CustomPathSitemapSource custom when source.GetType() == typeof(CustomPathSitemapSource) => JsonSerializer.SerializeToNode(custom, JsonOptions).AsObject(),
            ContentTypesSitemapSource content when source.GetType() == typeof(ContentTypesSitemapSource) => JsonSerializer.SerializeToNode(content, JsonOptions).AsObject(),
            _ => null,
        };
        configuration?.Remove("id");
        configuration?.Remove("lastUpdate");
        return new() { Id = source.Id, Type = source.GetType().Name, Supported = configuration is not null, Configuration = configuration };
    }
    private static SitemapSourceMutationResult Error(string key, string message) => new() { Errors = new() { [key] = [message] } };
}

internal sealed class SitemapSourceMutationResult
{
    public SitemapSource Source { get; init; }
    public bool Changed { get; init; }
    public bool NotFound { get; init; }
    public Dictionary<string, string[]> Errors { get; init; } = [];
}

/// <summary>A typed source write. Use the source schema to prepare its complete configuration.</summary>
public sealed class SitemapSourceDefinition
{
    /// <summary>Gets or sets a registered built-in source type name.</summary>
    public string Type { get; set; }
    /// <summary>Gets or sets the source configuration, excluding server-controlled IDs and timestamps.</summary>
    public JsonObject Configuration { get; set; }
}

/// <summary>A safe source readback. Unknown extension types expose only identity.</summary>
public sealed class SitemapSourceResponse
{
    /// <summary>Gets the source ID.</summary>
    public string Id { get; init; }
    /// <summary>Gets the source type.</summary>
    public string Type { get; init; }
    /// <summary>Gets whether this source has a typed contract.</summary>
    public bool Supported { get; init; }
    /// <summary>Gets the allowlisted typed configuration.</summary>
    public JsonObject Configuration { get; init; }
}
