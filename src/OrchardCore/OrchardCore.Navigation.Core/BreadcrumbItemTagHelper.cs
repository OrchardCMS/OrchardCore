using System.Net;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.AspNetCore.Routing;

namespace OrchardCore.Navigation.TagHelpers;

/// <summary>
/// Declares a single ancestor of a breadcrumb trail inline, as a child of the <c>breadcrumb</c> tag helper.
/// Ancestors are collected in declaration order. Their localized inner text can use data from the view model or
/// services injected into the view.
/// </summary>
/// <remarks>
/// When the admin breadcrumb setting is disabled, the parent skips its children entirely.
/// Link permissions are resolved by the parent helper after providers have run.
/// </remarks>
/// <example>
/// <code>
/// &lt;breadcrumb name="ContentsEdit" title="@T["Edit Article"]"&gt;
///     &lt;breadcrumb-item action="List" controller="Admin" area="OrchardCore.Contents"&gt;Manage Content&lt;/breadcrumb-item&gt;
/// &lt;/breadcrumb&gt;
/// </code>
/// </example>
[HtmlTargetElement("breadcrumb-item", ParentTag = "breadcrumb")]
public sealed class BreadcrumbItemTagHelper : TagHelper
{
    private IDictionary<string, string> _routeValues;

    /// <summary>
    /// The identifier of the node, used to build its shape alternates. See
    /// <see cref="BreadcrumbItem.Id"/>.
    /// </summary>
    [HtmlAttributeName("id")]
    public string Id { get; set; }

    /// <summary>
    /// The name of the permission the user must have for the node to be rendered as a link. The node is still rendered
    /// as plain text when the user lacks it, so that the trail stays complete. An unknown name is ignored.
    /// </summary>
    [HtmlAttributeName("permission")]
    public string PermissionName { get; set; }

    /// <summary>
    /// The url the node links to. It is ignored when <see cref="Action"/> is set.
    /// </summary>
    [HtmlAttributeName("url")]
    public string Url { get; set; }

    /// <summary>
    /// The action of the route the node links to.
    /// </summary>
    [HtmlAttributeName("action")]
    public string Action { get; set; }

    /// <summary>
    /// The controller of the route the node links to.
    /// </summary>
    [HtmlAttributeName("controller")]
    public string Controller { get; set; }

    /// <summary>
    /// The area of the route the node links to.
    /// </summary>
    [HtmlAttributeName("area")]
    public string Area { get; set; }

    /// <summary>
    /// Additional route values for the link, also supplied through <c>route-*</c> attributes.
    /// </summary>
    [HtmlAttributeName("route-values", DictionaryAttributePrefix = "route-")]
    public IDictionary<string, string> RouteValues
    {
        get => _routeValues ??= new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        set => _routeValues = value;
    }

    /// <summary>
    /// The resource passed to authorization when evaluating the node's permission.
    /// </summary>
    [HtmlAttributeName("resource")]
    public object Resource { get; set; }

    /// <summary>
    /// Whether the node may render a link. Set to false when the view has already determined that access is denied.
    /// </summary>
    [HtmlAttributeName("link-enabled")]
    public bool LinkEnabled { get; set; } = true;

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.SuppressOutput();

        if (!context.Items.TryGetValue(typeof(BreadcrumbItemTagHelper), out var value) || value is not BreadcrumbContext breadcrumbContext)
        {
            return;
        }

        var content = await output.GetChildContentAsync();
        // Razor has already encoded dynamic text. Store plain text so shapes and headings encode it only once.
        var text = WebUtility.HtmlDecode(content.GetContent().Trim());

        var item = new BreadcrumbItem
        {
            Text = text,
            Id = Id,
            Resource = Resource,
            LinkEnabled = LinkEnabled,
            PermissionName = PermissionName,
        };

        if (!string.IsNullOrEmpty(Action))
        {
            item.RouteValues = new RouteValueDictionary
            {
                ["action"] = Action,
            };

            if (!string.IsNullOrEmpty(Controller))
            {
                item.RouteValues["controller"] = Controller;
            }

            if (!string.IsNullOrEmpty(Area))
            {
                item.RouteValues["area"] = Area;
            }

            if (_routeValues is not null)
            {
                foreach (var (key, routeValue) in _routeValues)
                {
                    item.RouteValues[key] = routeValue;
                }
            }
        }
        else if (!string.IsNullOrEmpty(Url))
        {
            item.Url = Url;
        }

        breadcrumbContext.Items.Add(item);
    }
}
