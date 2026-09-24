using System.Collections.ObjectModel;
using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Admin.Models;

/// <summary>
/// The columns of an admin list, keyed by <see cref="AdminListColumn.Name"/> ignoring case, in the order they
/// were added. A provider finds a column by its name, so every column has one, and no other column of the list
/// has it: adding a column without a name, or a second column with the same name, throws.
/// </summary>
/// <remarks>
/// The name of a column is its key in the collection, so it is set before the column is added and does not
/// change afterwards.
/// </remarks>
public sealed class AdminListColumnCollection : KeyedCollection<string, AdminListColumn>
{
    public AdminListColumnCollection()
        : base(StringComparer.OrdinalIgnoreCase)
    {
    }

    /// <summary>
    /// Sorts the columns by <see cref="AdminListColumn.Position"/>, in place. A column without a position goes after
    /// all the positioned ones, and the columns sharing a position keep the order they were added in.
    /// </summary>
    public void SortByPosition()
    {
        // An insertion sort: a list has a handful of columns, it allocates nothing, and unlike List<T>.Sort it
        // keeps the order of the columns sharing a position. The items are moved without touching their keys.
        var items = Items;

        for (var i = 1; i < items.Count; i++)
        {
            var column = items[i];
            var position = GetPosition(column);
            var j = i - 1;

            while (j >= 0 && FlatPositionComparer.Instance.Compare(GetPosition(items[j]), position) > 0)
            {
                items[j + 1] = items[j];
                j--;
            }

            items[j + 1] = column;
        }
    }

    protected override string GetKeyForItem(AdminListColumn item)
        => item.Name;

    protected override void InsertItem(int index, AdminListColumn item)
    {
        EnsureName(item);

        if (Contains(item.Name))
        {
            throw new ArgumentException($"The list already has a '{item.Name}' column.", nameof(item));
        }

        base.InsertItem(index, item);
    }

    protected override void SetItem(int index, AdminListColumn item)
    {
        EnsureName(item);

        base.SetItem(index, item);
    }

    private static string GetPosition(AdminListColumn column)
        => string.IsNullOrWhiteSpace(column.Position) ? "after" : column.Position;

    private static void EnsureName(AdminListColumn item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(item.Name))
        {
            throw new ArgumentException("A column of an admin list needs a name.", nameof(item));
        }
    }
}
