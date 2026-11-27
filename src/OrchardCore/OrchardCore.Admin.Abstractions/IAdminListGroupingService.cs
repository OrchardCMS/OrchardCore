namespace OrchardCore.Admin;

/// <summary>
/// Resolves grouping before a page queries its rows and when the shared list renders.
/// </summary>
public interface IAdminListGroupingService
{
    /// <summary>
    /// Resolves the URL choice, then the browser preference, for the case-sensitive list name.
    /// Unavailable options resolve to None.
    /// </summary>
    AdminListGroupingSelection GetSelection(string listName);

    /// <summary>
    /// Generates a same-page URL preserving filters and layout and resetting the page number.
    /// </summary>
    string GetUrl(string listName, string grouping);
}
