namespace OrchardCore.Navigation;

/// <summary>
/// Updates a breadcrumb's ancestors after its inline children have been collected and before its explicit
/// title is appended as the current node. Providers can also supply all ancestors when no children are declared.
/// </summary>
public interface IBreadcrumbProvider
{
    /// <summary>
    /// Adds, removes or updates the ancestors of the named trail. Providers run in registration order, before
    /// positioning, link resolution and appending the title. The tag helper does not resolve or invoke providers
    /// when the admin breadcrumb setting is disabled.
    /// </summary>
    /// <param name="context">The named trail, mutable items and rendering context.</param>
    ValueTask BuildBreadcrumbAsync(BreadcrumbContext context);
}
