using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// Extends view-defined columns.
/// </summary>
public interface IAdminListColumnsBuilder
{
    /// <summary>
    /// Runs column providers against the view-defined columns, then sorts the result by position.
    /// </summary>
    /// <param name="listName">The name of the list, e.g. <c>Users</c>.</param>
    /// <param name="columns">The columns declared by the view.</param>
    /// <param name="data">Optional contextual values for column providers.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    /// <returns>The extended and sorted columns.</returns>
    Task<AdminListColumnsContext> BuildColumnsAsync(string listName, IEnumerable<AdminListColumn> columns, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default);
}
