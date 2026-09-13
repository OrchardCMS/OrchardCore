using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// Resolves the layout and the columns of an admin list.
/// </summary>
public interface IAdminListService
{
    /// <summary>
    /// Gets the layout to use for the given list, from the most specific source to the least: the layout asked
    /// for here, the one asked for in the query string, the one this user picked for this list before, and the
    /// default of the site. The last two only count when the site lets a user choose, see
    /// <see cref="AdminListOptions.AllowUserSelection"/>.
    /// </summary>
    /// <param name="listName">The name of the list, e.g. <c>Contents</c>.</param>
    /// <param name="requestedLayout">An explicit layout requested for this rendering. When not empty it takes precedence over everything else.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The layout name, never empty. Defaults to <see cref="AdminListConstants.DefaultLayout"/>.</returns>
    Task<string> GetLayoutAsync(string listName, string requestedLayout = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the layouts this site can render, discovered from the shape table: a layout is available when a
    /// shape named <c>AdminListLayout_Option__{Layout}</c> exists, e.g. <c>AdminListLayout-Table.Option.cshtml</c>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The layout names, in alphabetical order.</returns>
    Task<IList<string>> GetAvailableLayoutsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the layouts to offer a user for the given list, each with the address rendering the page with it,
    /// or nothing when the site keeps the choice to itself or can render a single layout.
    /// </summary>
    /// <remarks>
    /// The first caller of a request takes the offer: a page rendering the same list several times, e.g. one
    /// list per group, shows a single selector, and a page building its own gets it instead of its lists.
    /// </remarks>
    /// <param name="listName">The name of the list, e.g. <c>Contents</c>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The layouts, in alphabetical order, or an empty list.</returns>
    Task<IList<AdminListLayoutOption>> GetLayoutOptionsAsync(string listName, CancellationToken cancellationToken = default);

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
    /// <param name="data">
    /// What the page knows about this rendering and a provider may need to decide on a column, e.g. the content
    /// types the items are filtered by. It reaches the providers as <see cref="AdminListColumnsContext.Data"/>.
    /// </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    Task<IList<AdminListColumn>> GetColumnsAsync(string listName, IEnumerable<AdminListColumn> defaultColumns, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default);
}
