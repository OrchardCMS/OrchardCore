using OrchardCore.Admin.Models;

namespace OrchardCore.Admin;

/// <summary>
/// Builds the columns of an admin list from its <see cref="IAdminListColumnProvider"/> implementations.
/// </summary>
public interface IAdminListColumnsBuilder
{
    /// <summary>
    /// Builds the columns of the given list: every <see cref="IAdminListColumnProvider"/> adds, removes or changes
    /// them, then they are sorted by <see cref="AdminListColumn.Position"/>.
    /// </summary>
    /// <param name="listName">The name of the list, e.g. <c>Contents</c>.</param>
    /// <param name="data">
    /// What the page knows about this rendering and a provider may need to decide on a column, e.g. the content
    /// types the items are filtered by. It reaches the providers as <see cref="AdminListColumnsContext.Data"/>.
    /// </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The columns, empty when no provider declared any for the list.</returns>
    Task<IList<AdminListColumn>> BuildAsync(string listName, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default);
}
