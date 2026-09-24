using Microsoft.AspNetCore.Mvc.Rendering;

namespace OrchardCore.Navigation;

/// <summary>
/// Provides the items and rendering context of a breadcrumb to its providers.
/// </summary>
public sealed class BreadcrumbContext
{
    /// <summary>
    /// Creates the context for collecting and updating the ancestors of a named breadcrumb.
    /// </summary>
    /// <param name="name">The name of the trail.</param>
    /// <param name="title">The normalized text of the explicit page title appended after all providers have run.</param>
    /// <param name="items">The mutable list of ancestors shared by all providers.</param>
    /// <param name="viewContext">The context of the view rendering the trail.</param>
    /// <param name="showTrail">Whether the trail is visible rather than supplying only a title.</param>
    public BreadcrumbContext(string name, string title, List<BreadcrumbItem> items, ViewContext viewContext, bool showTrail)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(viewContext);

        Name = name;
        Title = title;
        Items = items;
        ViewContext = viewContext;
        ShowTrail = showTrail;
    }

    /// <summary>
    /// Gets the name of the trail, which a provider can use to restrict its changes to a particular breadcrumb.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the normalized text of the explicit page title. The tag helper appends it as the current node
    /// after all providers have run.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the mutable ancestors, initially populated in inline declaration order. Providers can add, insert,
    /// remove, replace, reorder or edit them. The final list order is preserved when rendering.
    /// The explicit title is not included until all providers have run.
    /// </summary>
    public List<BreadcrumbItem> Items { get; }

    /// <summary>
    /// Gets the context of the view rendering the trail.
    /// </summary>
    public ViewContext ViewContext { get; }

    /// <summary>
    /// Gets whether the trail is visible. The tag helper only creates this context and invokes providers for
    /// visible trails, so this value is always true when called by the tag helper.
    /// </summary>
    public bool ShowTrail { get; }
}
