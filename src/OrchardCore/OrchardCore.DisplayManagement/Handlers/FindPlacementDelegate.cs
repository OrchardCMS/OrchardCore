using OrchardCore.DisplayManagement.Descriptors;

namespace OrchardCore.DisplayManagement.Handlers
{
    /// <summary>
    /// A function that provides a <see cref="PlacementInfo"/> instance for a shape.
    /// </summary>
    /// <param name="shapeType">The shape type to render.</param>
    /// <param name="differentiator">
    /// A discriminator that differentiates this specific shape to the others of the same type.
    /// For instance multiple field shape smight be displayed in different locations even though
    /// they are of the same type.
    /// </param>
    /// <param name="displayType">The display type the content item owning the shape is rendered with.</param>
    /// <param name="context">The <see cref="IBuildShapeContext"/> in which the shape is placed.</param>
    /// <returns>The <see cref="PlacementInfo"/> to use or <see langword="null"/> if this function is not concerned.</returns>
    public delegate PlacementInfo FindPlacementDelegate(string shapeType, string differentiator, string displayType, IBuildShapeContext context);
}
