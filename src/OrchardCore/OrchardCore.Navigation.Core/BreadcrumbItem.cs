using Microsoft.AspNetCore.Routing;
using OrchardCore.Security.Permissions;

namespace OrchardCore.Navigation;

/// <summary>
/// Represents an ancestor declared inline or updated by a provider, or the current page's explicit title node.
/// Nodes are rendered in the order of the breadcrumb's item list.
/// </summary>
public class BreadcrumbItem
{
    /// <summary>
    /// Gets or sets the text to display for the node.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Gets or sets the optional identifier of the node. It is used to build the shape alternates of the node,
    /// It is not rendered as an HTML id.
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
    /// Gets or sets the final url the node links to. This property is computed by the parent tag helper
    /// based on <see cref="Url"/> or <see cref="RouteValues"/>. It is <c>null</c> when the node is not a link, which is
    /// the case for the current node and for a node the user is not authorized to reach.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// Gets or sets whether the node represents the page being rendered. The parent tag helper sets this
    /// property on the explicit title node appended after all providers have run.
    /// </summary>
    public bool IsCurrent { get; set; }

    /// <summary>
    /// Gets or sets whether the node may render a link. When false, it remains visible as plain text.
    /// </summary>
    public bool LinkEnabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the resource the node's <see cref="PermissionName"/> and <see cref="Permissions"/> are evaluated against.
    /// </summary>
    public object Resource { get; set; }

    /// <summary>
    /// Gets or sets the name of an additional permission required for the link. It is resolved only for a visible
    /// ancestor link, after providers have run. An unknown name is ignored.
    /// </summary>
    public string PermissionName { get; set; }

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
