namespace OrchardCore.Admin;

/// <summary>
/// Well-known admin list layouts and shape names, and the naming conventions used to discover them.
/// </summary>
/// <remarks>
/// A layout is a named presentation of an admin list. It is rendered by the <c>AdminList</c> shape using the
/// alternate <c>AdminList__{Layout}</c> (e.g. <c>AdminList-Table.cshtml</c>). A layout is made available in the
/// admin settings when a shape named <c>AdminListLayout_Option__{Layout}</c> exists (e.g. <c>AdminListLayout-Table.Option.cshtml</c>),
/// which mirrors how content field editors are discovered.
/// </remarks>
public static class AdminListConstants
{
    /// <summary>
    /// The default layout, rendering the items as a vertical list.
    /// </summary>
    public const string List = "List";

    /// <summary>
    /// The default layout, rendering the items as a vertical list.
    /// </summary>
    public const string DefaultLayout = List;

    /// <summary>
    /// Renders the items as a table, one column per <see cref="Models.AdminListColumn"/>.
    /// </summary>
    public const string Table = "Table";

    /// <summary>
    /// Renders the items with the same columns as <see cref="Table"/> but with a CSS grid instead of a table,
    /// so the header always lines up with the data.
    /// </summary>
    public const string Grid = "Grid";

    /// <summary>
    /// The shape type rendering an admin list.
    /// </summary>
    public const string ShapeType = "AdminList";

    /// <summary>
    /// The shape type rendering a single cell of an admin list with columns.
    /// </summary>
    public const string CellShapeType = "AdminListCell";

    /// <summary>
    /// The shape type rendering the toolbar of an admin list, above its rows.
    /// </summary>
    public const string ToolbarShapeType = "AdminListToolbar";

    /// <summary>
    /// The shape type rendering the search bar of an admin list.
    /// </summary>
    public const string SearchShapeType = "AdminListSearch";

    /// <summary>
    /// The name of the property every part of a list carries, so a template can be overridden for one list.
    /// </summary>
    public const string ListNameProperty = "ListName";

    /// <summary>
    /// The name of the property every part of a list carries with the layout rendering it, so a template can
    /// be overridden for one layout, e.g. a toolbar that only applies to <see cref="Grid"/>.
    /// </summary>
    public const string ListLayoutProperty = "ListLayout";

    /// <summary>
    /// The prefix of the shapes that declare an available layout, e.g. <c>AdminListLayout_Option__Table</c>.
    /// </summary>
    public const string OptionShapePrefix = "AdminListLayout_Option__";
}
