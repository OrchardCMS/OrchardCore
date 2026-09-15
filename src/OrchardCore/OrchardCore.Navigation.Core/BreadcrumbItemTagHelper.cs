using Microsoft.AspNetCore.Razor.TagHelpers;

namespace OrchardCore.Navigation.TagHelpers;

/// <summary>
/// Declares a single node of a breadcrumb trail inline, as a child of the <c>breadcrumb</c> tag helper. It lets a
/// screen whose trail is static describe it in the view, without an <see cref="IBreadcrumbProvider"/>. The providers
/// still run over the trail, so another module can add to, remove from or reorder its nodes.
/// </summary>
/// <example>
/// <code>
/// &lt;breadcrumb name="ContentsEdit"&gt;
///     &lt;breadcrumb-item action="List" controller="Admin" area="OrchardCore.Contents"&gt;Manage Content&lt;/breadcrumb-item&gt;
///     &lt;breadcrumb-item&gt;Edit Article&lt;/breadcrumb-item&gt;
/// &lt;/breadcrumb&gt;
/// </code>
/// </example>
[HtmlTargetElement("breadcrumb-item", ParentTag = "breadcrumb")]
public sealed class BreadcrumbItemTagHelper : TagHelper
{
    /// <summary>
    /// The identifier of the node, used to build its shape alternates and to let a provider find it. See
    /// <see cref="BreadcrumbItem.Id"/>.
    /// </summary>
    [HtmlAttributeName("id")]
    public string Id { get; set; }

    /// <summary>
    /// The relative position of the node among the other nodes of the trail. e.g., <c>10</c>, <c>before</c>, <c>end</c>.
    /// </summary>
    [HtmlAttributeName("position")]
    public string Position { get; set; }

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

    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        var content = await output.GetChildContentAsync();

        output.SuppressOutput();

        // The parent 'breadcrumb' tag helper puts the collecting list in the shared items before it renders its
        // children. A stray 'breadcrumb-item' outside a 'breadcrumb' therefore just renders nothing.
        if (!context.Items.TryGetValue(typeof(BreadcrumbItemTagHelper), out var value) || value is not List<BreadcrumbItem> items)
        {
            return;
        }

        var item = new BreadcrumbItem();
        var builder = new BreadcrumbItemBuilder(item);

        builder.Text(content.GetContent().Trim());

        if (!string.IsNullOrEmpty(Id))
        {
            builder.Id(Id);
        }

        if (!string.IsNullOrEmpty(Position))
        {
            builder.Position(Position);
        }

        if (!string.IsNullOrEmpty(Action))
        {
            builder.Action(Action, Controller, Area);
        }
        else if (!string.IsNullOrEmpty(Url))
        {
            builder.Url(Url);
        }

        items.Add(item);
    }
}
