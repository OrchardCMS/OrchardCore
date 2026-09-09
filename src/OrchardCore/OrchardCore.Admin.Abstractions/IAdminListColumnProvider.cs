namespace OrchardCore.Admin;

/// <summary>
/// Allows a module to add, remove or reorder the columns of an admin list.
/// </summary>
public interface IAdminListColumnProvider
{
    /// <summary>
    /// Alters the columns of the list identified by <see cref="AdminListColumnsContext.ListName"/>.
    /// </summary>
    Task BuildAsync(AdminListColumnsContext context);
}
