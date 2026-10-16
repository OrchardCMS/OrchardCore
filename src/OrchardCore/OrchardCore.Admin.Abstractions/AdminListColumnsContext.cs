using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// The view-defined columns of a list, extended by <see cref="IAdminListColumnsProvider"/> implementations.
/// </summary>
public sealed class AdminListColumnsContext
{
    /// <summary>
    /// Creates a context with the columns declared by the view.
    /// </summary>
    public AdminListColumnsContext(string listName, IEnumerable<AdminListColumn> columns, IReadOnlyDictionary<string, object> data = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(listName);
        ArgumentNullException.ThrowIfNull(columns);

        ListName = listName;
        Data = data ?? new Dictionary<string, object>();

        foreach (var column in columns)
        {
            Columns.Add(column);
        }
    }

    /// <summary>
    /// Gets the name of the list being rendered.
    /// </summary>
    public string ListName { get; }

    /// <summary>
    /// Gets the columns to add, remove or modify before they are sorted by position.
    /// </summary>
    public AdminListColumnCollection Columns { get; } = new();

    /// <summary>
    /// Gets contextual data supplied by the page, such as the selected content types.
    /// </summary>
    public IReadOnlyDictionary<string, object> Data { get; }

    /// <summary>
    /// Gets a contextual value when it has the expected type.
    /// </summary>
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
    /// Gets a contextual value or the supplied default.
    /// </summary>
    public T GetData<T>(string key, T defaultValue = default)
        => TryGetData<T>(key, out var value) ? value : defaultValue;

    /// <summary>
    /// Finds a column by its technical name.
    /// </summary>
    public AdminListColumn Find(string name)
        => name != null && Columns.TryGetValue(name, out var column) ? column : null;

    /// <summary>
    /// Removes a column by its technical name.
    /// </summary>
    public bool Remove(string name)
        => name != null && Columns.Remove(name);
}
