namespace OrchardCore.Admin;

/// <summary>
/// Extends the columns declared by an admin list's view. Row display drivers supply the cell shapes,
/// using <c>SummaryAdminGrid</c> zone placement, for example <c>Title:10</c>.
/// </summary>
public interface IAdminListColumnsProvider
{
    /// <summary>
    /// Adds, removes or changes columns of the applicable lists before they are sorted.
    /// </summary>
    /// <param name="context">The list name, view-defined columns and contextual data.</param>
    Task PopulateAsync(AdminListColumnsContext context);
}
