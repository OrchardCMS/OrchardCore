namespace OrchardCore.Navigation;

/// <summary>
/// Describes the nodes of one or more breadcrumb trails. Every provider registered on the tenant is called for every
/// breadcrumb that is rendered, which is how a module adds a node to a trail owned by another module.
/// </summary>
public interface IBreadcrumbProvider
{
    /// <summary>
    /// Adds the nodes of the breadcrumb identified by <see cref="BreadcrumbBuilder.Name"/> to the builder.
    /// </summary>
    /// <param name="builder">The builder collecting the nodes of the breadcrumb being rendered.</param>
    ValueTask BuildBreadcrumbAsync(BreadcrumbBuilder builder);
}
