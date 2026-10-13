using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement;
using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Admin;

/// <summary>
/// The context passed to <see cref="IAdminListColumnDriver{TModel}.BuildCellsAsync"/>: a row of the list, and the
/// shape holding its cells, whose zones are named after the columns.
/// </summary>
public class AdminListCellsContext : BuildDisplayContext
{
    private readonly AdminListColumnsContext _columns;

    /// <summary>
    /// Creates the context of a row of the list whose columns are described by <paramref name="columns"/>.
    /// </summary>
    /// <param name="columns">The columns of the list, as the drivers declared them.</param>
    /// <param name="cells">The shape whose zones, named after the columns, receive the shapes of the cells of the row.</param>
    /// <param name="row">The row shape the page built, e.g. a <c>SummaryAdmin</c> shape.</param>
    public AdminListCellsContext(AdminListColumnsContext columns, IZoneHolding cells, IShape row)
        : base(cells, AdminListConstants.CellDisplayType, string.Empty, columns?.ShapeFactory, columns?.Layout, columns?.Updater)
    {
        ArgumentNullException.ThrowIfNull(columns);
        ArgumentNullException.ThrowIfNull(cells);

        _columns = columns;
        Cells = cells;
        Row = row;
    }

    /// <summary>
    /// The name of the list being rendered, e.g. <c>Contents</c>.
    /// </summary>
    public string ListName => _columns.ListName;

    /// <summary>
    /// What the page rendering the list passed along, see <see cref="AdminListColumnsContext.Data"/>.
    /// </summary>
    public IReadOnlyDictionary<string, object> Data => _columns.Data;

    /// <summary>
    /// The columns of the list, in the order they are rendered. A shape placed in a zone no column is named after
    /// is not rendered, e.g. when another driver removed the column.
    /// </summary>
    public IReadOnlyList<AdminListColumn> Columns => _columns.Columns;

    /// <summary>
    /// The shape holding the cells of the row, one zone per column, named after it.
    /// </summary>
    public IZoneHolding Cells { get; }

    /// <summary>
    /// The row shape the page built, e.g. a <c>SummaryAdmin</c> shape, whose zones the columns render with
    /// <see cref="AdminListColumn.Zones"/>.
    /// </summary>
    public IShape Row { get; }

    /// <summary>
    /// Gets the column with the given name, or <see langword="null"/> when the list does not have it.
    /// </summary>
    public AdminListColumn Find(string name)
        => _columns.Find(name);
}
