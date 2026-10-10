using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using OrchardCore.Admin.Models;
using OrchardCore.DisplayManagement.Descriptors;
using OrchardCore.Mvc.Utilities;

namespace OrchardCore.Admin.Services;

/// <summary>
/// Tells how the layouts with columns render the cells of a column: through an <c>AdminListCell</c> shape when a
/// template overrides them, otherwise directly, which saves a shape for every row and column.
/// </summary>
public static class AdminListCellRenderings
{
    // Found once per list and column of a shape table, which is built once per theme and replaced when the tenant
    // reloads, so a table that is gone takes its entries with it.
    private static readonly ConditionalWeakTable<ShapeTable, ConcurrentDictionary<(string ListName, string Column), AdminListCellRendering>> _renderings = new();

    /// <summary>
    /// Gets how the cells of the given column of the given list are rendered with the given shape table.
    /// </summary>
    public static AdminListCellRendering Get(ShapeTable shapeTable, string listName, string column)
    {
        ArgumentNullException.ThrowIfNull(shapeTable);
        ArgumentException.ThrowIfNullOrEmpty(column);

        return _renderings
            .GetValue(shapeTable, static _ => new ConcurrentDictionary<(string ListName, string Column), AdminListCellRendering>())
            .GetOrAdd((listName ?? string.Empty, column), static (key, table) => Resolve(table, key.ListName, key.Column), shapeTable);
    }

    private static AdminListCellRendering Resolve(ShapeTable shapeTable, string listName, string column)
    {
        // The alternates of a cell, from the most specific to the base shape, see AdminListShapeTableProvider.
        if ((!string.IsNullOrEmpty(listName) && IsOverridden(shapeTable, $"{AdminListConstants.CellShapeType}__{listName.ToSafeName()}__{column}")) ||
            IsOverridden(shapeTable, $"{AdminListConstants.CellShapeType}__{column}") ||
            IsOverridden(shapeTable, AdminListConstants.CellShapeType))
        {
            return AdminListCellRendering.Shape;
        }

        return string.Equals(column, AdminListColumns.ActionsName, StringComparison.OrdinalIgnoreCase)
            ? AdminListCellRendering.Actions
            : AdminListCellRendering.Zones;
    }

    // The templates of this module render what the layouts render directly, so any other binding overrides them: a
    // template of a theme or of another module, or a shape declared in code.
    private static bool IsOverridden(ShapeTable shapeTable, string shapeType)
        => shapeTable.Bindings.TryGetValue(shapeType, out var binding) && !IsShipped(binding);

    /// <summary>
    /// Whether the binding is a template of this module, e.g. <c>Areas/OrchardCore.Admin/Views/AdminListCell.cshtml</c>.
    /// </summary>
    public static bool IsShipped(ShapeBinding binding)
        => binding?.BindingSource is { } source &&
           ("/" + source.Replace('\\', '/')).Contains("/OrchardCore.Admin/Views/", StringComparison.OrdinalIgnoreCase);
}
