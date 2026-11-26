namespace OrchardCore.Admin;

/// <summary>
/// Selects rows from an ordered grouped result, without leaving unused capacity at group boundaries.
/// </summary>
public static class AdminListGrouping
{
    /// <summary>
    /// Takes an absolute row page and retains the heading metadata for every group represented on that page.
    /// </summary>
    /// <param name="groups">The complete groups, in display order.</param>
    /// <param name="skip">The number of rows before the page.</param>
    /// <param name="take">The maximum number of rows on the page.</param>
    public static IReadOnlyList<AdminListGroup> Page(IEnumerable<AdminListGroup> groups, int skip, int take)
    {
        ArgumentNullException.ThrowIfNull(groups);
        ArgumentOutOfRangeException.ThrowIfNegative(skip);
        ArgumentOutOfRangeException.ThrowIfNegative(take);
        var result = new List<AdminListGroup>();
        foreach (var group in groups)
        {
            if (take == 0)
            {
                break;
            }

            var items = group.Source.Cast<object>().ToArray();
            if (skip >= items.Length)
            {
                skip -= items.Length;
                continue;
            }

            var page = items.Skip(skip).Take(take).ToArray();
            result.Add(new AdminListGroup { Key = group.Key, Title = group.Title, TotalCount = group.TotalCount, Source = page });
            skip = 0;
            take -= page.Length;
        }

        return result;
    }
}
