using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.Endpoints.Management;

/// <summary>The complete placement fields to apply to the latest and published widget versions.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LayerWidgetPlacement
{
    /// <summary>An existing layer name, resolved case-insensitively and stored with its canonical case.</summary>
    [Required]
    public string Layer { get; init; }
    /// <summary>A configured zone using its exact case.</summary>
    [Required]
    public string Zone { get; init; }
    /// <summary>The finite ordering position; smaller values render first within a zone.</summary>
    [Required]
    public double? Position { get; init; }
    /// <summary>Whether the widget title is rendered. Omission defaults to false.</summary>
    public bool RenderTitle { get; init; }

    internal LayerMetadata ToMetadata() => new() { Layer = Layer, Zone = Zone, Position = Position ?? double.NaN, RenderTitle = RenderTitle };
}

/// <summary>The placement of one stored widget version, without its content body.</summary>
public sealed class LayerWidgetResponse
{
    /// <summary>The stable widget content item identity.</summary>
    public string ContentItemId { get; init; }
    /// <summary>The identity of this content version.</summary>
    public string ContentItemVersionId { get; init; }
    /// <summary>The widget content type.</summary>
    public string ContentType { get; init; }
    /// <summary>The widget display text.</summary>
    public string DisplayText { get; init; }
    /// <summary>Whether this version is published.</summary>
    public bool Published { get; init; }
    /// <summary>The complete placement fields for this version.</summary>
    public LayerWidgetPlacement Placement { get; init; }
}

/// <summary>Filters and pagination for widgets already attached to layers.</summary>
public sealed class LayerWidgetListRequest
{
    /// <summary>The version selection: latest (default) or published.</summary>
    public string Version { get; init; }
    /// <summary>An optional case-insensitive layer name filter.</summary>
    public string Layer { get; init; }
    /// <summary>An optional exact zone name filter.</summary>
    public string Zone { get; init; }
    /// <summary>The zero-based offset; defaults to zero.</summary>
    public int? Skip { get; init; }
    /// <summary>The page size, between 1 and 200; defaults to 50.</summary>
    public int? Take { get; init; }
}

/// <summary>A page of authorized widget placements.</summary>
public sealed class LayerWidgetListResponse
{
    /// <summary>The applied offset.</summary>
    public int Skip { get; init; }
    /// <summary>The applied page size.</summary>
    public int Take { get; init; }
    /// <summary>The number of matching widgets the caller may view, before paging.</summary>
    public int TotalCount { get; init; }
    /// <summary>The authorized placements ordered by zone, position and content item identity.</summary>
    public IReadOnlyList<LayerWidgetResponse> Items { get; init; } = [];
}
