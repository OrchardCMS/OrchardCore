namespace OrchardCore.Admin;

/// <summary>
/// Extends the columns declared by an admin list's view. Row display drivers supply the cell shapes,
/// using <c>SummaryAdminGrid</c> zone placement, for example <c>Title:10</c>.
/// </summary>
public interface IAdminListColumnsProvider
{
    /// <summary>
    /// Adds, removes or changes columns of the lists this provider is registered for before they are sorted.
    /// Global providers run before list-specific providers and must tolerate different column sets.
    /// </summary>
    /// <param name="context">The list name, view-defined columns and contextual data.</param>
    Task PopulateAsync(AdminListColumnsContext context);
}
