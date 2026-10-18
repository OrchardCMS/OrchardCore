namespace OrchardCore.Admin;

/// <summary>
/// The supported presentations of an admin list.
/// </summary>
public enum AdminListLayout
{
    /// <summary>
    /// Renders each item's <c>SummaryAdmin</c> shape as a complete row.
    /// </summary>
    List,

    /// <summary>
    /// Renders the zones of each item's <c>SummaryAdminGrid</c> shape in the declared columns.
    /// </summary>
    Grid,
}
