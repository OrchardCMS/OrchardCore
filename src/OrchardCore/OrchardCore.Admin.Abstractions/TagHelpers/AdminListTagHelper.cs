using System.Collections;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.TagHelpers;

/// <summary>
/// Creates or configures an AdminList shape from model items, a context or an existing shape,
/// then displays it after collecting child column and layout declarations.
/// </summary>
[HtmlTargetElement("admin-list", Attributes = "shape")]
[HtmlTargetElement("admin-list", Attributes = "context")]
[HtmlTargetElement("admin-list", Attributes = "items")]
public sealed class AdminListTagHelper : TagHelper
{
    private readonly IDisplayHelper _displayHelper;
    private readonly IAdminListFactory _listFactory;

    /// <summary>
    /// Creates a helper that renders through the standard shape pipeline.
    /// </summary>
    /// <param name="displayHelper">The shape display helper.</param>
    /// <param name="listFactory">The admin list factory.</param>
    public AdminListTagHelper(IDisplayHelper displayHelper, IAdminListFactory listFactory)
    {
        _displayHelper = displayHelper;
        _listFactory = listFactory;
    }

    /// <summary>
    /// The AdminList shape to configure. It must not have been initialized or displayed yet.
    /// </summary>
    public IShape Shape { get; set; }

    /// <summary>
    /// Optional list data and region shapes. The tag creates its AdminList shape, not the controller.
    /// </summary>
    public AdminListContext Context { get; set; }

    /// <summary>
    /// The list identity used by column providers, layout preferences and shape alternates.
    /// Required with a context or items; when supplied with an existing shape, overrides its name.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Model items to render through their display manager. These are not row shapes.
    /// </summary>
    public IEnumerable Items { get; set; }

    /// <summary>
    /// Optional display model type; otherwise inferred from the typed items, including empty collections.
    /// </summary>
    public Type ItemType { get; set; }

    /// <summary>
    /// Optional pager shape when supplying items directly.
    /// </summary>
    public IShape Pager { get; set; }

    /// <summary>
    /// Attributes of the rows container, also supplied with rows-* attributes such as rows-id.
    /// </summary>
    [HtmlAttributeName("rows-attributes", DictionaryAttributePrefix = "rows-")]
    public IDictionary<string, string> RowsAttributes { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Whether to build the default toolbar. When omitted, preserves the shape's setting.
    /// </summary>
    public bool? ShowToolbar { get; set; }

    /// <summary>
    /// Whether the default toolbar offers select-all. When omitted, preserves the shape's setting.
    /// </summary>
    public bool? ShowSelectAll { get; set; }

    /// <summary>
    /// Whether to offer layout selection. When omitted, preserves the shape's setting.
    /// </summary>
    public bool? ShowLayoutSelector { get; set; }

    /// <summary>
    /// Optional forced layout. Child layout declarations do not force a layout.
    /// </summary>
    public AdminListLayout? Layout { get; set; }

    /// <summary>
    /// CSS classes for List rows. Layout-specific options can override these.
    /// </summary>
    public string ItemCssClass { get; set; }

    /// <summary>
    /// The localized message displayed when there are no rows.
    /// </summary>
    public IHtmlContent EmptyMessage { get; set; }

    /// <summary>
    /// Bulk actions offered by the default toolbar.
    /// </summary>
    public IList<SelectListItem> BulkActions { get; set; }

    /// <inheritdoc />
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (Shape != null && Context != null)
        {
            throw new InvalidOperationException("Supply either an admin list context or an existing shape, not both.");
        }

        if (Name != null || Shape == null)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        }

        Shape ??= await _listFactory.CreateAsync(Name, Context ?? new AdminListContext());
        if (Shape.Metadata.Type != AdminListConstants.ShapeType || Shape.GetProperty<bool>("AdminListInitialized"))
        {
            throw new InvalidOperationException("The admin-list tag requires an uninitialized AdminList shape.");
        }

        if (Name != null)
        {
            Shape.Properties[nameof(Name)] = Name;
        }

        if (ShowToolbar.HasValue)
        {
            Shape.Properties[nameof(ShowToolbar)] = ShowToolbar.Value;
        }

        if (ShowSelectAll.HasValue)
        {
            Shape.Properties[nameof(ShowSelectAll)] = ShowSelectAll.Value;
        }

        if (ShowLayoutSelector.HasValue)
        {
            Shape.Properties[nameof(ShowLayoutSelector)] = ShowLayoutSelector.Value;
        }

        if (Layout != null)
        {
            Shape.Properties[nameof(Layout)] = Layout;
        }

        if (ItemCssClass != null)
        {
            Shape.Properties[nameof(ItemCssClass)] = ItemCssClass;
        }

        if (EmptyMessage != null)
        {
            Shape.Properties[nameof(EmptyMessage)] = EmptyMessage;
        }

        if (BulkActions != null)
        {
            Shape.Properties[nameof(BulkActions)] = BulkActions;
        }

        if (Items != null)
        {
            Shape.Properties["Source"] = Items;
        }

        if (ItemType != null)
        {
            Shape.Properties[nameof(ItemType)] = ItemType;
        }

        if (Pager != null)
        {
            Shape.Properties[nameof(Pager)] = Pager;
        }

        if (RowsAttributes.Count > 0)
        {
            var attributes = new Dictionary<string, string>(Shape.GetProperty<IDictionary<string, string>>(nameof(RowsAttributes))
                ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
            foreach (var attribute in RowsAttributes)
            {
                attributes[attribute.Key] = attribute.Value;
            }

            Shape.Properties[nameof(RowsAttributes)] = attributes;
        }

        var columns = new AdminListColumnCollection();
        foreach (var column in Shape.GetProperty<IEnumerable<AdminListColumn>>("Columns") ?? [])
        {
            columns.Add(column);
        }

        context.Items[typeof(AdminListTagHelper)] = Shape;
        context.Items[typeof(AdminListColumnTagHelper)] = columns;
        context.Items[typeof(AdminListLayoutTagHelper)] = new HashSet<string>(StringComparer.Ordinal);
        await output.GetChildContentAsync();
        Shape.Properties["Columns"] = columns;

        output.TagName = null;
        output.Attributes.Clear();
        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(Shape));
    }
}
