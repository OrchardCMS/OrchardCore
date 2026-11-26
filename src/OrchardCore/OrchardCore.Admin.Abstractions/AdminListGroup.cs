using System.Collections;

namespace OrchardCore.Admin;

/// <summary>
/// A contiguous group of source items. Its title and total refer to the filtered result, not just one page.
/// </summary>
public class AdminListGroup
{
    /// <summary>
    /// The stable, culture-independent identity of the group.
    /// </summary>
    public string Key { get; set; }

    /// <summary>
    /// The plain-text heading, encoded by the group header template.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// The source items, in display order. A query-backed page supplies only its page's items.
    /// </summary>
    public IEnumerable Source { get; set; }

    /// <summary>
    /// The number of matching rows in the complete group, including other pages.
    /// </summary>
    public int TotalCount { get; set; }
}
