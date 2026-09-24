using Microsoft.AspNetCore.Html;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin;

/// <summary>
/// Describes an admin list for <see cref="IAdminListFactory"/> to render. Only the name is required: the
/// columns are built by the <see cref="IAdminListColumnProvider"/> of the list, and the layout is resolved for
/// the list unless the page sets one.
/// </summary>
public sealed class AdminListContext
{
    /// <summary>
    /// Creates the description of the list with the given name.
    /// </summary>
    /// <param name="name">The name of the list, e.g. <c>Contents</c>.</param>
    public AdminListContext(string name)
    {
        ArgumentException.ThrowIfNullOrEmpty(name);

        Name = name;
    }

    /// <summary>
    /// The name of the list, e.g. <c>Contents</c>. It selects the columns the providers declare for the list,
    /// the layout a user picked for it, and the <c>AdminList__{Name}</c> alternates.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// The layout to render the list with, e.g. <see cref="AdminListConstants.List"/>. When empty, the layout
    /// the user picked for this list or the default of the site is used. A page sets it when it can only be
    /// rendered one way, e.g. while its rows are reordered by dragging them.
    /// </summary>
    public string Layout { get; set; }

    /// <summary>
    /// The row shapes, usually built with the <c>SummaryAdmin</c> display type. The classes and attributes of a
    /// row shape, e.g. <c>data-filter-value</c> for a client-side search, are rendered on its row.
    /// </summary>
    public IEnumerable<IShape> Rows { get; set; } = [];

    /// <summary>
    /// What the page knows about this rendering and a column provider may need to decide on a column, e.g. the
    /// content types the items are filtered by. It reaches the providers as <see cref="AdminListColumnsContext.Data"/>.
    /// A list documents the keys it fills, e.g. <c>ContentsAdminList.ContentTypesKey</c>.
    /// </summary>
    public Dictionary<string, object> Data { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// The options editor shape whose <c>Summary</c> and <c>Actions</c> zones are rendered above the rows.
    /// </summary>
    public IShape Header { get; set; }

    /// <summary>
    /// Alternative to <see cref="Header"/>: a shape rendered as is above the rows, e.g. <c>AdminListToolbar</c>.
    /// </summary>
    public IShape Toolbar { get; set; }

    /// <summary>
    /// The search bar of the list, e.g. the <c>AdminListSearch</c> shape, rendered in the action bar above the list.
    /// </summary>
    public IShape Search { get; set; }

    /// <summary>
    /// The buttons of the page, e.g. "Add", rendered beside the search in the action bar.
    /// </summary>
    public IShape Actions { get; set; }

    /// <summary>
    /// The pager shape.
    /// </summary>
    public IShape Pager { get; set; }

    /// <summary>
    /// The page size selector, so a layout can place it apart from the pager.
    /// </summary>
    public IShape PageSize { get; set; }

    /// <summary>
    /// Whether the list offers the user its other layouts, when the site lets a user choose. A page that renders
    /// several lists, e.g. one per group, or that places the selector itself, turns it off on its lists.
    /// </summary>
    public bool ShowLayoutSelector { get; set; } = true;

    /// <summary>
    /// Attributes rendered on the element wrapping the rows, e.g. the id a sortable list needs. A <c>class</c>
    /// joins the ones the layout gives the element.
    /// </summary>
    public IDictionary<string, string> RowsAttributes { get; set; }

    /// <summary>
    /// The CSS classes of each row in the <c>List</c> layout, defaults to <c>list-group-item list-group-item-action</c>.
    /// </summary>
    public string ItemCssClass { get; set; }

    /// <summary>
    /// The message displayed when there are no rows, defaults to "No results found.".
    /// </summary>
    public IHtmlContent EmptyMessage { get; set; }
}
