using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// The context passed to <see cref="IAdminListColumnProvider"/>.
/// </summary>
public class AdminListColumnsContext
{
    private static readonly IReadOnlyDictionary<string, object> _emptyData
        = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public AdminListColumnsContext(string listName, IList<AdminListColumn> columns, IReadOnlyDictionary<string, object> data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);
        ArgumentNullException.ThrowIfNull(columns);

        ListName = listName;
        Columns = columns;
        Data = data ?? _emptyData;
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
    /// What the page rendering the list passed along, e.g. the content types and the stereotypes the content
    /// items are filtered by. Empty when the page passed nothing. A list documents the keys it fills, e.g.
    /// <c>ContentsAdminList.ContentTypesKey</c>.
    /// </summary>
    public IReadOnlyDictionary<string, object> Data { get; }

    /// <summary>
    /// Gets the value the page passed under the given key, when it holds the expected type.
    /// </summary>
    /// <returns><see langword="true"/> when the key is there and holds a <typeparamref name="T"/>.</returns>
    public bool TryGetData<T>(string key, out T value)
    {
        if (key != null && Data.TryGetValue(key, out var item) && item is T typed)
        {
            value = typed;

            return true;
        }

        value = default;

        return false;
    }

    /// <summary>
    /// Gets the value the page passed under the given key, or <paramref name="defaultValue"/> when the key is
    /// not there or holds another type.
    /// </summary>
    public T GetData<T>(string key, T defaultValue = default)
        => TryGetData<T>(key, out var value) ? value : defaultValue;

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
