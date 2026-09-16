namespace OrchardCore.Layers.Models;

/// <summary>The outcome of a layer mutation.</summary>
public enum LayerMutationStatus
{
    /// <summary>The requested mutation was applied.</summary>
    Success,
    /// <summary>The name does not satisfy layer naming constraints.</summary>
    InvalidName,
    /// <summary>A layer with the same case-insensitive name already exists.</summary>
    Conflict,
    /// <summary>No layer with the requested name exists.</summary>
    NotFound,
    /// <summary>A latest widget references the layer, preventing deletion.</summary>
    Referenced,
}

/// <summary>A transport-independent result for admin and API layer mutations.</summary>
public sealed class LayerMutationResult
{
    /// <summary>Gets the outcome without prescribing an HTTP response or admin notification.</summary>
    public LayerMutationStatus Status { get; init; }
    /// <summary>Gets the affected or conflicting layer, when available.</summary>
    public Layer Layer { get; init; }
    /// <summary>Gets a localized name validation or conflict message, when applicable.</summary>
    public string Error { get; init; }
}
