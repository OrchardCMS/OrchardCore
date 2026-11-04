using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrchardCore.DisplayManagement.Descriptors.ShapePlacementStrategy;

namespace OrchardCore.Placements.Endpoints.Management;

/// <summary>A complete ordered set of placement overrides for one shape type.</summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PlacementDefinition
{
    /// <summary>Gets the stable, case-insensitive shape type key.</summary>
    [Required, MaxLength(256)]
    public string ShapeType { get; init; }
    /// <summary>Gets the ordered placement rules. An empty array removes the override when updating.</summary>
    [Required]
    public PlacementNode[] Nodes { get; init; }
}

/// <summary>Filters and pages stored shape placements.</summary>
public sealed class PlacementListRequest
{
    /// <summary>Gets an optional case-insensitive shape type search.</summary>
    public string Search { get; init; }
    /// <summary>Gets the zero-based offset, defaulting to zero.</summary>
    [Range(0, int.MaxValue)]
    public int? Skip { get; init; }
    /// <summary>Gets the page size, defaulting to 50 and limited to 200.</summary>
    [Range(1, 200)]
    public int? Take { get; init; }
}

/// <summary>A page of stored placement definitions.</summary>
public sealed class PlacementListResponse
{
    /// <summary>Gets the applied offset.</summary>
    public int Skip { get; init; }
    /// <summary>Gets the applied page size.</summary>
    public int Take { get; init; }
    /// <summary>Gets the number of matching shape types before paging.</summary>
    public int TotalCount { get; init; }
    /// <summary>Gets the placement definitions in this page.</summary>
    public IReadOnlyList<PlacementDefinition> Items { get; init; } = [];
}

/// <summary>Reports validation without rendering or persistence.</summary>
public sealed class PlacementValidationResponse
{
    /// <summary>Gets whether all supplied rules are valid.</summary>
    public bool IsValid { get; init; }
    /// <summary>Gets errors keyed by request property path.</summary>
    public IDictionary<string, string[]> Errors { get; init; } = new Dictionary<string, string[]>();
}
