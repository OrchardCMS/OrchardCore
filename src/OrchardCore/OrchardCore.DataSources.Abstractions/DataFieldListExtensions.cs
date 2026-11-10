namespace OrchardCore.DataSources;

/// <summary>
/// Provides lookups over lists of <see cref="DataField"/>.
/// </summary>
public static class DataFieldListExtensions
{
    /// <summary>
    /// Finds the index of a field by its technical name.
    /// </summary>
    /// <param name="fields">The fields to search.</param>
    /// <param name="name">The technical field name.</param>
    /// <returns>The zero-based index, or <c>-1</c> when there is no such field.</returns>
    public static int IndexOf(this IReadOnlyList<DataField> fields, string name)
    {
        ArgumentNullException.ThrowIfNull(fields);

        if (string.IsNullOrEmpty(name))
        {
            return -1;
        }

        for (var index = 0; index < fields.Count; index++)
        {
            if (string.Equals(fields[index].Name, name, StringComparison.Ordinal))
            {
                return index;
            }
        }

        return -1;
    }

    /// <summary>
    /// Finds a field by its technical name.
    /// </summary>
    /// <param name="fields">The fields to search.</param>
    /// <param name="name">The technical field name.</param>
    /// <returns>The field, or <see langword="null"/> when there is no such field.</returns>
    public static DataField Find(this IReadOnlyList<DataField> fields, string name)
    {
        var index = fields.IndexOf(name);

        return index < 0 ? null : fields[index];
    }
}
