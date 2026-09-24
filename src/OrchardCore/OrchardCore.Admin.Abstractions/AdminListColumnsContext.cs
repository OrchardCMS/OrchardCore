using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// The context passed to <see cref="IAdminListColumnProvider"/>.
/// </summary>
public class AdminListColumnsContext
{
    private static readonly IReadOnlyDictionary<string, object> _emptyData
        = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public AdminListColumnsContext(string listName, IReadOnlyDictionary<string, object> data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);

        ListName = listName;
        Data = data ?? _emptyData;
    }

    /// <summary>
    /// The name of the list being rendered, e.g. <c>Contents</c>.
    /// </summary>
    public string ListName { get; }

    /// <summary>
    /// The columns the providers declared so far, starting with the ones of the module owning the list. Providers
    /// alter this collection in place. A column name is unique in the list: adding a second column with the same
    /// name throws. The order of this collection does not matter: the columns are sorted by
    /// <see cref="AdminListColumn.Position"/> once every provider ran.
    /// </summary>
    public AdminListColumnCollection Columns { get; } = new();

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
        => name != null && Columns.TryGetValue(name, out var column) ? column : null;

    /// <summary>
    /// Removes the column with the given name, if any.
    /// </summary>
    /// <returns><see langword="true"/> when a column was removed.</returns>
    public bool Remove(string name)
        => name != null && Columns.Remove(name);
}
