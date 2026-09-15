namespace OrchardCore.Admin;

/// <summary>
/// Allows a module to add, remove or reorder the columns of an admin list.
/// </summary>
public interface IAdminListColumnProvider
{
    /// <summary>
    /// Alters the columns of the list identified by <see cref="AdminListColumnsContext.ListName"/>.
    /// </summary>
    /// <param name="context">The columns of the list being rendered.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default);
}
