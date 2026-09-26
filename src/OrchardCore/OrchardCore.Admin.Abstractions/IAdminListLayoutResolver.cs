using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// Resolves the layout of an admin list and the layouts a user can switch it to.
/// </summary>
public interface IAdminListLayoutResolver
{
    /// <summary>
    /// Gets the layout to use for the given list, from the most specific source to the least: the one asked for
    /// in the query string, the one this user picked for this list before, and the default of the site. The first
    /// two only count when the site lets a user choose, see <see cref="AdminListOptions.AllowUserSelection"/>,
    /// and a layout asked for in the query string is remembered for the list.
    /// </summary>
    /// <param name="listName">The name of the list, e.g. <c>Contents</c>.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The layout name, never empty. Defaults to <see cref="AdminListConstants.List"/>.</returns>
    Task<string> GetLayoutAsync(string listName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the layouts this site can render, discovered from the shape table: a layout is available when a
    /// shape named <c>AdminListLayout_Option__{Layout}</c> exists, e.g. <c>AdminListLayout-Grid.Option.cshtml</c>.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The layout names as their option shapes declare them, in alphabetical order.</returns>
    Task<IList<string>> GetAvailableLayoutsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the layouts to offer a user, each with the address rendering the current page with it, or nothing
    /// when the site keeps the choice to itself or can render a single layout.
    /// </summary>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The layouts, in alphabetical order, or an empty list.</returns>
    Task<IList<AdminListLayoutOption>> GetLayoutOptionsAsync(CancellationToken cancellationToken = default);
}
