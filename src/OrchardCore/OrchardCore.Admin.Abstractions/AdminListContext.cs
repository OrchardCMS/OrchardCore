using System.Collections;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin;

/// <summary>
/// Describes the items, contextual data and region shapes for <see cref="IAdminListFactory"/> to render.
/// The view supplies the list identity, columns and presentation properties; row display drivers place
/// shapes in zones for the selected display type.
/// Use <see cref="AdminListContext{TModel}"/> for a typed source with an automatically selected item type.
/// </summary>
public class AdminListContext
{
    /// <summary>
    /// The model items to render. The list invokes their display manager after selecting the layout.
    /// Leave unset when supplying prebuilt rows or a custom row builder.
    /// </summary>
    public IEnumerable Source { get; set; }

    /// <summary>
    /// The display model type. When omitted, inferred from the source's unambiguous IEnumerable&lt;T&gt; interface,
    /// including empty collections. Specify a base type when its display manager should render derived items.
    /// </summary>
    public Type ItemType { get; set; }

    /// <summary>
    /// Gets or sets already-built row shapes for the selected layout: <c>SummaryAdmin</c> for List or
    /// <c>SummaryAdminGrid</c> for layouts with columns. Use <see cref="BuildRowsAsync"/> to defer this choice.
    /// </summary>
    public IList<IShape> Rows { get; set; } = [];

    /// <summary>
    /// Gets or sets a builder invoked once after resolving the layout, with <c>SummaryAdmin</c> for List
    /// or <c>SummaryAdminGrid</c> for layouts with columns. Its result replaces <see cref="Rows"/>.
    /// Leave unset when the page supplies already-built rows.
    /// </summary>
    public Func<string, Task<IList<IShape>>> BuildRowsAsync { get; set; }

    /// <summary>
    /// Gets or sets the columns declared by the rendering view, before column providers extend them.
    /// </summary>
    public IEnumerable<Models.AdminListColumn> Columns { get; set; } = [];

    /// <summary>
    /// The layout to render the list with, e.g. <see cref="AdminListLayout.List"/>. When null, the layout
    /// the user picked for this list or the default of the site is used. A page sets it when it can only be
    /// rendered one way, e.g. while its rows are reordered by dragging them.
    /// </summary>
    public AdminListLayout? Layout { get; set; }

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
    /// Alternative to <see cref="Header"/>: a shape rendered as is above the rows, for a list that needs a toolbar
    /// of its own. When the page sets neither, the factory builds the <c>AdminListToolbar</c> shape, see
    /// <see cref="ShowToolbar"/>.
    /// </summary>
    public IShape Toolbar { get; set; }

    /// <summary>
    /// Whether the factory builds the <c>AdminListToolbar</c> shape when the page sets neither <see cref="Header"/>
    /// nor <see cref="Toolbar"/>: the item count, taken from the rows and <see cref="Pager"/>, the
    /// select-all checkbox and the <see cref="BulkActions"/>. Defaults to <see langword="true"/>. A list without a
    /// toolbar, e.g. the recipes, turns it off.
    /// </summary>
    public bool ShowToolbar { get; set; } = true;

    /// <summary>
    /// The bulk actions offered by the toolbar the factory builds. The <see cref="SelectListItem.Value"/> of each
    /// is the name of the action posted with the form.
    /// </summary>
    public IList<SelectListItem> BulkActions { get; set; }

    /// <summary>
    /// Whether the toolbar the factory builds precedes the item count with the select-all checkbox. Defaults to
    /// <see langword="true"/>. A list whose rows cannot be selected turns it off.
    /// </summary>
    public bool ShowSelectAll { get; set; } = true;

    /// <summary>
    /// A shape rendered at the end of the toolbar the factory builds, e.g. the filters of the list.
    /// </summary>
    public IShape ToolbarActions { get; set; }

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
    /// Presentation settings used only by the List layout. Does not force that layout.
    /// </summary>
    public Models.AdminListLayoutOptions ListOptions { get; set; }

    /// <summary>
    /// Presentation settings used only by the Grid layout. Does not force that layout.
    /// </summary>
    public Models.AdminListLayoutOptions GridOptions { get; set; }

    /// <summary>
    /// The message displayed when there are no rows, defaults to "No results found.".
    /// </summary>
    public IHtmlContent EmptyMessage { get; set; }
}
