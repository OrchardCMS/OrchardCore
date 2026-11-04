using System.Security.Claims;
using OrchardCore.Layers.Models;

namespace OrchardCore.Layers.Services;

/// <summary>Validates and updates widget placement without publishing draft content.</summary>
public interface ILayerWidgetService
{
    /// <summary>Returns the configured tenant zone names, preserving their case.</summary>
    Task<string[]> GetZonesAsync();
    /// <summary>Validates the layer, configured zone and finite ordering position.</summary>
    Task<Dictionary<string, string[]>> ValidateAsync(LayerMetadata placement);
    /// <summary>
    /// Updates the latest and published versions after checking all resource permissions.
    /// Position-only moves require existing metadata and retain each version's layer and title setting.
    /// Full updates can attach existing widgets. Neither operation publishes draft content.
    /// </summary>
    Task<LayerWidgetMutationResult> UpdateAsync(ClaimsPrincipal user, string contentItemId, LayerMetadata placement, bool positionOnly = false);
}

/// <summary>Outcome of an attempted widget placement change.</summary>
public enum LayerWidgetMutationStatus
{
    /// <summary>The requested placement is stored, including an equivalent retry.</summary>
    Success,
    /// <summary>No latest content item exists.</summary>
    NotFound,
    /// <summary>The caller cannot change every affected version.</summary>
    Forbidden,
    /// <summary>The content type or requested placement is invalid.</summary>
    Invalid,
}

/// <summary>A placement result and any field validation errors.</summary>
public sealed class LayerWidgetMutationResult
{
    /// <summary>The mutation outcome.</summary>
    public LayerWidgetMutationStatus Status { get; init; }
    /// <summary>The latest widget placement after a successful operation.</summary>
    public LayerMetadata Placement { get; init; }
    /// <summary>Validation errors keyed by placement field.</summary>
    public Dictionary<string, string[]> Errors { get; init; } = [];
}
