using OrchardCore.DisplayManagement.Handlers;

namespace OrchardCore.DisplayManagement.Descriptors;

/// <summary>
/// Defines a service capable of providing a contextualized <see cref="IPlacementInfoResolver"/> for a given shape building context.
/// </summary>
public interface IShapePlacementProvider
{
    /// <summary>
    /// Builds a contextualized <see cref="IPlacementInfoResolver"/>.
    /// </summary>
    /// <param name="context">The <see cref="IBuildShapeContext"/> for which we need a placement resolver.</param>
    /// <returns>An instance of <see cref="IPlacementInfoResolver"/> for the current context or <see langword="null"/> if this provider is not concerned.</returns>
    Task<IPlacementInfoResolver> BuildPlacementInfoResolverAsync(IBuildShapeContext context);
}
