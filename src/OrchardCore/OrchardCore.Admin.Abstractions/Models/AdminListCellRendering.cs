namespace OrchardCore.Admin.Models;

/// <summary>
/// How a layout with columns renders the cells of a column, as the <c>CellRenderings</c> property of the
/// <c>AdminList</c> shape tells it, one value per column.
/// </summary>
/// <remarks>
/// A cell is rendered through an <c>AdminListCell</c> shape only when a template overrides it, so a list does not
/// create a shape for every row and column to render the templates the Admin module ships.
/// </remarks>
public enum AdminListCellRendering
{
    /// <summary>
    /// The zones of the row the column names, rendered as they are, like <c>AdminListCell.cshtml</c> does.
    /// </summary>
    Zones,

    /// <summary>
    /// The actions of the row, rendered by the <c>AdminListActions</c> shape, like <c>AdminListCell-Actions.cshtml</c>
    /// does for the column named <see cref="AdminListColumns.ActionsName"/>.
    /// </summary>
    Actions,

    /// <summary>
    /// An <c>AdminListCell</c> shape, because a template overrides the cells of the column: <c>AdminListCell</c>,
    /// <c>AdminListCell-{Column}</c> or <c>AdminListCell-{ListName}-{Column}</c>.
    /// </summary>
    Shape,
}
