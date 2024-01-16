namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// Represents a class capable of resolving <see cref="PlacementInfo"/> of Shapes
/// </summary>
public interface IPlacementInfoResolver
{
    /// <summary>
    /// Resolves <see cref="PlacementInfo"/> for the provided <see cref="ShapePlacementContext"/>
    /// </summary>
    /// <param name="placementContext">The <see cref="ShapePlacementContext"/></param>
    /// <returns>An <see cref="PlacementInfo"/> or <see langword="null"/> if not concerned.</returns>
    PlacementInfo ResolvePlacement(ShapePlacementContext placementContext);
}
