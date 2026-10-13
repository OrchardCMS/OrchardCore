using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Admin;

/// <summary>
/// Declares the columns of the admin lists of <typeparamref name="TModel"/> items, e.g. the users, and builds what
/// they render: the header of a column once per list, and its cell once per row. A driver usually derives from
/// <see cref="AdminListColumnDriver{TModel}"/>.
/// </summary>
/// <typeparam name="TModel">The items of the list, e.g. <c>User</c>, as the page passes them with their rows.</typeparam>
/// <remarks>
/// A driver is registered for the items it renders, e.g.
/// <c>services.AddAdminListColumnDriver&lt;User, UsersAdminListColumnDriver&gt;()</c>, so it only runs, and is only
/// created, when a list of these items is rendered. The drivers run in the order their features depend on each
/// other, so the module owning a list adds its columns before the modules depending on it change or remove them.
/// Adding a column does not depend on that order: the columns are sorted by
/// <see cref="Models.AdminListColumn.Position"/> once every driver ran.
/// </remarks>
public interface IAdminListColumnDriver<TModel>
{
    /// <summary>
    /// Adds, removes or changes the columns of the list identified by <see cref="AdminListColumnsContext.ListName"/>.
    /// It runs once per list, before any row is rendered, so every row has the same columns, and so does a page
    /// without rows.
    /// </summary>
    /// <param name="context">The columns of the list being rendered.</param>
    /// <returns>
    /// The shapes rendered in the headers of the columns, placed by the name of their column, e.g.
    /// <c>.Location("Scheduled")</c>, or <see langword="null"/> when the titles of the columns are enough.
    /// </returns>
    Task<IDisplayResult> BuildColumnsAsync(AdminListColumnsContext context);

    /// <summary>
    /// Builds the cells of a row. It only runs for the layouts with columns, e.g. <c>Grid</c>: the <c>List</c>
    /// layout renders the row shape whole.
    /// </summary>
    /// <param name="model">The item of the row.</param>
    /// <param name="context">The row being rendered.</param>
    /// <returns>
    /// The shapes rendered in the cells of the row, placed by the name of their column, e.g.
    /// <c>.Location("Scheduled:5")</c>, after the zones of the row the column renders, or <see langword="null"/>.
    /// </returns>
    Task<IDisplayResult> BuildCellsAsync(TModel model, AdminListCellsContext context);
}
