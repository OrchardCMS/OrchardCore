using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.TagHelpers;

/// <summary>
/// Declares List or Grid presentation settings without selecting the layout.
/// Grid declarations can contain admin-list-column children.
/// </summary>
[HtmlTargetElement("admin-list-list", ParentTag = "admin-list")]
[HtmlTargetElement("admin-list-grid", ParentTag = "admin-list")]
public sealed class AdminListLayoutTagHelper : TagHelper
{
    /// <summary>
    /// CSS classes added to the rows container for this layout only.
    /// </summary>
    public string RowsClass { get; set; }

    /// <summary>
    /// CSS classes for rows in this layout only.
    /// </summary>
    public string ItemCssClass { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (!context.Items.TryGetValue(typeof(AdminListTagHelper), out var value) || value is not IShape shape)
        {
            throw new InvalidOperationException("Admin list layout declarations must be inside an admin-list tag.");
        }

        var property = string.Equals(context.TagName, "admin-list-grid", StringComparison.OrdinalIgnoreCase) ? "GridOptions" : "ListOptions";
        var layouts = (HashSet<string>)context.Items[typeof(AdminListLayoutTagHelper)];
        if (!layouts.Add(property))
        {
            throw new InvalidOperationException($"The admin-list tag can only contain one {context.TagName} declaration.");
        }

        var options = shape.GetProperty<AdminListLayoutOptions>(property) ?? new AdminListLayoutOptions();
        if (RowsClass != null)
        {
            options.RowsCssClass = RowsClass;
        }

        if (ItemCssClass != null)
        {
            options.ItemCssClass = ItemCssClass;
        }

        shape.Properties[property] = options;

        await output.GetChildContentAsync();
        output.SuppressOutput();
    }
}
