using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// Resolves the layout and the columns of an admin list.
/// </summary>
public interface IAdminListService
{
    /// <summary>
    /// Gets the layout to use for the given list.
    /// </summary>
    /// <param name="listName">The name of the list, e.g. <c>Contents</c>.</param>
    /// <param name="requestedLayout">An explicit layout requested for this rendering, e.g. from the query string. When not empty it takes precedence over the configured default.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The layout name, never empty. Defaults to <see cref="AdminListConstants.DefaultLayout"/>.</returns>
    Task<string> GetLayoutAsync(string listName, string requestedLayout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the layout of the row actions to use for the given list.
    /// </summary>
    /// <param name="listName">The name of the list, e.g. <c>Contents</c>, or <see langword="null"/> when the row is rendered outside a named list.</param>
    /// <param name="requestedLayout">An explicit layout requested for this rendering. When not empty it takes precedence over the configured default.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The layout name, never empty. Defaults to <see cref="AdminListActionsLayouts.DefaultLayout"/>.</returns>
    Task<string> GetActionsLayoutAsync(string listName = null, string requestedLayout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the columns of the given list, starting from the defaults provided by the list owner and letting
    /// each <see cref="IAdminListColumnProvider"/> add, remove or reorder them.
    /// </summary>
    /// <param name="listName">The name of the list, e.g. <c>Contents</c>.</param>
    /// <param name="defaultColumns">The columns defined by the owner of the list.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    Task<IList<AdminListColumn>> GetColumnsAsync(string listName, IEnumerable<AdminListColumn> defaultColumns, CancellationToken cancellationToken = default);
}
