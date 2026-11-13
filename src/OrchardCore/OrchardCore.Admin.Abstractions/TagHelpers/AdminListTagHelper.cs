using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin.TagHelpers;

/// <summary>
/// Creates an AdminList shape from a context,
/// then displays it after collecting child column and layout declarations.
/// </summary>
[HtmlTargetElement("admin-list", Attributes = "context")]
public sealed class AdminListTagHelper : TagHelper
{
    private readonly IDisplayHelper _displayHelper;
    private readonly IShapeFactory _shapeFactory;

    /// <summary>
    /// Creates a helper that renders through the standard shape pipeline.
    /// </summary>
    /// <param name="displayHelper">The shape display helper.</param>
    /// <param name="shapeFactory">The shape factory.</param>
    public AdminListTagHelper(IDisplayHelper displayHelper, IShapeFactory shapeFactory)
    {
        _displayHelper = displayHelper;
        _shapeFactory = shapeFactory;
    }

    /// <summary>
    /// The items, list data and region shapes. The tag creates its AdminList shape, not the controller.
    /// </summary>
    public AdminListContext Context { get; set; }

    /// <summary>
    /// The list identity used by column providers, layout preferences and shape alternates.
    /// Required for every rendering.
    /// </summary>
    public string Name { get; set; }

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
        ArgumentException.ThrowIfNullOrWhiteSpace(Name);
        ArgumentNullException.ThrowIfNull(Context);

        var shape = await _shapeFactory.CreateAsync(AdminListConstants.ShapeType, Arguments.From(new
        {
            Name,
            Context.Source,
            Context.ItemType,
            Context.Columns,
            Context.Data,
            Context.Layout,
            Context.ShowLayoutSelector,
            Context.ShowToolbar,
            Context.ShowSelectAll,
            Context.BulkActions,
            Context.ToolbarActions,
            Context.Header,
            Context.Toolbar,
            Context.Search,
            Context.Actions,
            Context.Pager,
            Context.PageSize,
            Context.RowsAttributes,
            Context.ItemCssClass,
            Context.ListOptions,
            Context.GridOptions,
            Context.EmptyMessage,
        }));

        if (ShowToolbar.HasValue)
        {
            shape.Properties[nameof(ShowToolbar)] = ShowToolbar.Value;
        }

        if (ShowSelectAll.HasValue)
        {
            shape.Properties[nameof(ShowSelectAll)] = ShowSelectAll.Value;
        }

        if (ShowLayoutSelector.HasValue)
        {
            shape.Properties[nameof(ShowLayoutSelector)] = ShowLayoutSelector.Value;
        }

        if (Layout != null)
        {
            shape.Properties[nameof(Layout)] = Layout;
        }

        if (ItemCssClass != null)
        {
            shape.Properties[nameof(ItemCssClass)] = ItemCssClass;
        }

        if (EmptyMessage != null)
        {
            shape.Properties[nameof(EmptyMessage)] = EmptyMessage;
        }

        if (BulkActions != null)
        {
            shape.Properties[nameof(BulkActions)] = BulkActions;
        }

        if (RowsAttributes.Count > 0)
        {
            var attributes = new Dictionary<string, string>(shape.GetProperty<IDictionary<string, string>>(nameof(RowsAttributes))
                ?? new Dictionary<string, string>(), StringComparer.OrdinalIgnoreCase);
            foreach (var attribute in RowsAttributes)
            {
                attributes[attribute.Key] = attribute.Value;
            }

            shape.Properties[nameof(RowsAttributes)] = attributes;
        }

        var columns = new AdminListColumnCollection();
        foreach (var column in shape.GetProperty<IEnumerable<AdminListColumn>>("Columns") ?? [])
        {
            columns.Add(column);
        }

        context.Items[typeof(AdminListTagHelper)] = shape;
        context.Items[typeof(AdminListColumnTagHelper)] = columns;
        context.Items[typeof(AdminListLayoutTagHelper)] = new HashSet<string>(StringComparer.Ordinal);
        await output.GetChildContentAsync();
        shape.Properties["Columns"] = columns;

        output.TagName = null;
        output.Attributes.Clear();
        output.Content.SetHtmlContent(await _displayHelper.ShapeExecuteAsync(shape));
    }
}
