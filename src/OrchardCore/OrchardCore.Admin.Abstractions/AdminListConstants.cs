namespace OrchardCore.Admin;

/// <summary>
/// Well-known admin list shape and property names.
/// </summary>
public static class AdminListConstants
{
    /// <summary>
    /// The shape type rendering an admin list.
    /// </summary>
    public const string ShapeType = "AdminList";

    /// <summary>
    /// The shape type rendering a single cell of an admin list with columns.
    /// </summary>
    public const string CellShapeType = "AdminListCell";

    /// <summary>
    /// The shape type rendering the actions of an admin list row.
    /// </summary>
    public const string ActionsShapeType = "AdminListActions";

    /// <summary>
    /// The shape type rendering the toolbar of an admin list, above its rows.
    /// </summary>
    public const string ToolbarShapeType = "AdminListToolbar";

    /// <summary>
    /// The shape type rendering the search bar of an admin list.
    /// </summary>
    public const string SearchShapeType = "AdminListSearch";

    /// <summary>
    /// The shape type rendering the layouts a user can switch a list to.
    /// </summary>
    public const string LayoutSelectorShapeType = "AdminListLayoutSelector";

    /// <summary>
    /// The name of the property every part of a list carries, so a template can be overridden for one list.
    /// </summary>
    public const string ListNameProperty = "ListName";

    /// <summary>
    /// The name of the property every part of a list carries with the layout rendering it, so a template can
    /// be overridden for one layout, e.g. a toolbar that only applies to <see cref="AdminListLayout.Grid"/>.
    /// </summary>
    public const string ListLayoutProperty = "ListLayout";
}
