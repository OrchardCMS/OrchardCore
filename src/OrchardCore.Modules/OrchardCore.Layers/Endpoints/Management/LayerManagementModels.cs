using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrchardCore.Rules.Services;

namespace OrchardCore.Layers.Endpoints.Management;

/// <summary>
/// A complete layer definition. Conditions replace the entire existing rule.
/// </summary>
[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class LayerDefinitionDto
{
    /// <summary>Gets the stable layer name. Renaming is not supported by the management API.</summary>
    [Required]
    [MaxLength(256)]
    public string Name { get; init; }
    /// <summary>Gets the optional description.</summary>
    public string Description { get; init; }
    /// <summary>Gets the complete rule children. An empty rule does not match any request.</summary>
    public IReadOnlyList<RuleConditionDefinition> Conditions { get; init; } = [];
}

/// <summary>Filters and pages layer definitions.</summary>
public sealed class LayerListRequest
{
    /// <summary>Gets an optional case-insensitive name or description search.</summary>
    public string Search { get; init; }
    /// <summary>Gets the zero-based offset, defaulting to zero.</summary>
    [Range(0, int.MaxValue)]
    public int? Skip { get; init; }
    /// <summary>Gets the page size, defaulting to 50 and limited to 200.</summary>
    [Range(1, 200)]
    public int? Take { get; init; }
}

/// <summary>A page of layer definitions and the total number of matching layers.</summary>
public sealed class LayerListResponse
{
    /// <summary>Gets the applied offset.</summary>
    public int Skip { get; init; }
    /// <summary>Gets the applied page size.</summary>
    public int Take { get; init; }
    /// <summary>Gets the number of matching layers before paging.</summary>
    public int TotalCount { get; init; }
    /// <summary>Gets the layer definitions in this page.</summary>
    public IReadOnlyList<LayerDefinitionDto> Items { get; init; } = [];
}
