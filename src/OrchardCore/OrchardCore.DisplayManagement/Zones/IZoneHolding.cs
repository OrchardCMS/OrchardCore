namespace OrchardCore.DisplayManagement.Zones;

public interface IZoneHolding : IShape
{
    Zones Zones { get; }

    /// <summary>
    /// Removes shape-valued properties and cached rendered content without replacing this shape or its metadata.
    /// </summary>
    /// <remarks>
    /// Shape-valued properties are treated as zones, as in layout rendering.
    /// Retrieve zones again through <see cref="Zones"/> after resetting. Previously retrieved zone shapes
    /// no longer belong to this shape.
    /// </remarks>
    void ResetZones();
}
