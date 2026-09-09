using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// The context passed to <see cref="IAdminListColumnProvider"/>.
/// </summary>
public class AdminListColumnsContext
{
    public AdminListColumnsContext(string listName, IList<AdminListColumn> columns)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);
        ArgumentNullException.ThrowIfNull(columns);

        ListName = listName;
        Columns = columns;
    }

    /// <summary>
    /// The name of the list being rendered, e.g. <c>Contents</c>.
    /// </summary>
    public string ListName { get; }

    /// <summary>
    /// The columns, initialized with the defaults of the list owner. Providers alter this collection in place.
    /// The order of this collection does not matter: the columns are sorted by <see cref="AdminListColumn.Position"/>
    /// once every provider ran.
    /// </summary>
    public IList<AdminListColumn> Columns { get; }

    /// <summary>
    /// Gets the column with the given name, or <see langword="null"/>.
    /// </summary>
    public AdminListColumn Find(string name)
        => Columns.FirstOrDefault(column => string.Equals(column.Name, name, StringComparison.OrdinalIgnoreCase));

    /// <summary>
    /// Removes the column with the given name, if any.
    /// </summary>
    /// <returns><see langword="true"/> when a column was removed.</returns>
    public bool Remove(string name)
    {
        var column = Find(name);

        return column != null && Columns.Remove(column);
    }
}
