using System.Collections.ObjectModel;

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

    private static void EnsureName(AdminListColumn item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (string.IsNullOrWhiteSpace(item.Name))
        {
            throw new ArgumentException("A column of an admin list needs a name.", nameof(item));
        }
    }
}
