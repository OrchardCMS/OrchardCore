using OrchardCore.DisplayManagement.Zones;

namespace OrchardCore.Admin;

/// <summary>
/// Builds the columns of an admin list, and the cells of its rows, with the
/// <see cref="IAdminListColumnDriver{TModel}"/> implementations of its items.
/// </summary>
public interface IAdminListColumnsBuilder
{
    /// <summary>
    /// Builds the columns of the given list: every driver of <typeparamref name="TModel"/> adds, removes or
    /// changes them and builds the shapes of their headers, then they are sorted by
    /// <see cref="Models.AdminListColumn.Position"/>.
    /// </summary>
    /// <typeparam name="TModel">The items of the list, e.g. <c>User</c>.</typeparam>
    /// <param name="listName">The name of the list, e.g. <c>Users</c>.</param>
    /// <param name="data">
    /// What the page knows about this rendering and a driver may need to decide on a column, e.g. the content
    /// types the items are filtered by. It reaches the drivers as <see cref="AdminListColumnsContext.Data"/>.
    /// </param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>
    /// The context the drivers filled: its <see cref="AdminListColumnsContext.Columns"/>, empty when no driver
    /// declared any for the list, and its <see cref="AdminListColumnsContext.Headers"/>.
    /// </returns>
    Task<AdminListColumnsContext> BuildColumnsAsync<TModel>(string listName, IReadOnlyDictionary<string, object> data = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Builds the cells of the given rows: every driver of <typeparamref name="TModel"/> places shapes in the
    /// zones of a row's cells, named after the columns.
    /// </summary>
    /// <typeparam name="TModel">The items of the list, e.g. <c>User</c>.</typeparam>
    /// <param name="columns">The columns of the list, as <see cref="BuildColumnsAsync"/> built them.</param>
    /// <param name="rows">The rows of the list.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    /// <returns>The cells of each row, in the order of <paramref name="rows"/>.</returns>
    Task<IList<IZoneHolding>> BuildCellsAsync<TModel>(AdminListColumnsContext columns, IEnumerable<AdminListRow<TModel>> rows, CancellationToken cancellationToken = default);
}
