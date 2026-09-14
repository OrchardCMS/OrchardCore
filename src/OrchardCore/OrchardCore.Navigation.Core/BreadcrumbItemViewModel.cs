using OrchardCore.DisplayManagement;

namespace OrchardCore.Navigation;

/// <summary>
/// The shape of a single node of a breadcrumb trail.
/// </summary>
[GenerateShape]
public sealed partial class BreadcrumbItemViewModel
{
    public BreadcrumbItemViewModel()
    {
        Metadata.Type = "BreadcrumbItem";
    }

    /// <summary>
    /// Gets or sets the name of the breadcrumb this node belongs to.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the node being rendered.
    /// </summary>
    public BreadcrumbItem Item { get; set; }

    /// <summary>
    /// Gets or sets the breadcrumb shape this node belongs to.
    /// </summary>
    public IShape Breadcrumb { get; set; }

    /// <summary>
    /// Gets or sets the text to display for the node.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the url the node links to, or <c>null</c> when the node is not a link.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// Gets or sets whether the node represents the page being rendered.
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Gets or sets the zero based index of the node in the trail.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Gets or sets the html tag wrapping the text of the current node, e.g. <c>h1</c>. It is <c>null</c> when the
    /// node is not the current one, or when the breadcrumb renders no heading.
    /// </summary>
    public string Heading { get; set; }
}
