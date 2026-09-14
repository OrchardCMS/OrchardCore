using Microsoft.AspNetCore.Routing;
using OrchardCore.DisplayManagement;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Navigation;

/// <summary>
/// Represents a single node of a breadcrumb trail described by an <see cref="IBreadcrumbProvider"/> implementation.
/// </summary>
public class BreadcrumbItem : IPositioned
{
    /// <summary>
    /// Gets or sets the text to display for the node.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the optional identifier of the node. It is used to build the shape alternates of the node,
    /// and it allows another provider to find the node in order to change or remove it.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// Gets or sets the optional url the node links to. It is ignored when <see cref="RouteValues"/> is set.
    /// </summary>
    public string Url { get; set; }

    /// <summary>
    /// Gets or sets the optional route values used to generate the url of the node.
    /// </summary>
    public RouteValueDictionary RouteValues { get; set; }

    /// <summary>
    /// Gets or sets the final url the node links to. This property is computed by the <see cref="IBreadcrumbManager"/>
    /// based on <see cref="Url"/> or <see cref="RouteValues"/>. It is <c>null</c> when the node is not a link, which is
    /// the case for the current node and for a node the user is not authorized to reach.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// Gets or sets the relative position of the node among the other nodes of the trail. e.g., 10, 0, "before", "end".
    /// </summary>
    public string Position { get; set; }

    /// <summary>
    /// Gets or sets whether the node represents the page being rendered. The <see cref="IBreadcrumbManager"/> sets this
    /// property on the last node of the trail.
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Gets or sets the resource the <see cref="Permissions"/> of the node are evaluated against.
    /// </summary>
    public object Resource { get; set; }

    /// <summary>
    /// Gets the list of <see cref="Permission"/> objects the user must all have for the node to be rendered as a link.
    /// The node is still rendered as plain text when the user lacks one of them, so that the trail stays complete.
    /// </summary>
    public List<Permission> Permissions { get; } = [];

    /// <summary>
    /// Gets the css classes to render with the node.
    /// </summary>
    public List<string> Classes { get; } = [];
}
