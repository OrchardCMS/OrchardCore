using OrchardCore.DisplayManagement.Handlers;
using OrchardCore.DisplayManagement.Views;

namespace OrchardCore.Admin;

/// <summary>
/// The base class of an <see cref="IAdminListColumnDriver{TModel}"/>. Like a display driver, it builds its shapes
/// with <c>Initialize</c>, <c>Factory</c>, <c>View</c> or <c>Combine</c>, and places them with <c>Location</c>, where
/// the zone is the name of a column.
/// </summary>
/// <typeparam name="TModel">The items of the list, e.g. <c>User</c>.</typeparam>
/// <example>
/// A column of the users, filled by the zones of the <c>User_SummaryAdmin</c> shape, and another one filled by a
/// shape of its own:
/// <code>
/// public override IDisplayResult Columns(AdminListColumnsContext context)
/// {
///     context.Columns.Add(new AdminListColumn { Name = "Name", Position = "10", Title = S["Name"], Zones = ["Header"] });
///     context.Columns.Add(new AdminListColumn { Name = "LastLogin", Position = "20", Title = S["Last login"], Width = AdminListColumn.AutoWidth });
///
///     return null;
/// }
///
/// public override IDisplayResult Cells(User user, AdminListCellsContext context)
/// {
///     return Initialize&lt;UserLastLoginViewModel&gt;("UserLastLogin_AdminListCell", model => model.User = user)
///         .Location("LastLogin");
/// }
/// </code>
/// </example>
public abstract class AdminListColumnDriver<TModel> : DisplayDriverBase, IAdminListColumnDriver<TModel>
{
    /// <summary>
    /// Whether the driver takes part in the given list. Every list of <typeparamref name="TModel"/> items by
    /// default; a driver declaring the columns of one list only, e.g. when several lists render the same items,
    /// returns <see langword="true"/> for it alone.
    /// </summary>
    /// <param name="listName">The name of the list, e.g. <c>Contents</c>.</param>
    public virtual bool CanHandleList(string listName)
        => true;

    Task<IDisplayResult> IAdminListColumnDriver<TModel>.BuildColumnsAsync(AdminListColumnsContext context)
        => CanHandleList(context.ListName) ? ColumnsAsync(context) : Task.FromResult<IDisplayResult>(null);

    Task<IDisplayResult> IAdminListColumnDriver<TModel>.BuildCellsAsync(TModel model, AdminListCellsContext context)
        => CanHandleList(context.ListName) ? CellsAsync(model, context) : Task.FromResult<IDisplayResult>(null);

    /// <summary>
    /// Adds, removes or changes the columns of the list, and returns the shapes rendered in their headers.
    /// </summary>
    /// <param name="context">The columns of the list being rendered.</param>
    public virtual Task<IDisplayResult> ColumnsAsync(AdminListColumnsContext context)
        => Task.FromResult(Columns(context));

    /// <inheritdoc cref="ColumnsAsync(AdminListColumnsContext)"/>
    public virtual IDisplayResult Columns(AdminListColumnsContext context)
        => null;

    /// <summary>
    /// Builds the cells of a row, for the layouts with columns.
    /// </summary>
    /// <param name="model">The item of the row.</param>
    /// <param name="context">The row being rendered.</param>
    public virtual Task<IDisplayResult> CellsAsync(TModel model, AdminListCellsContext context)
        => Task.FromResult(Cells(model, context));

    /// <inheritdoc cref="CellsAsync(TModel, AdminListCellsContext)"/>
    public virtual IDisplayResult Cells(TModel model, AdminListCellsContext context)
        => null;
}
