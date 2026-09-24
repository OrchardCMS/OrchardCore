namespace OrchardCore.Admin;

/// <summary>
/// Adds, removes or changes the columns of an admin list. The module owning a list declares its columns with a
/// provider, and the other modules add to them the same way.
/// </summary>
/// <remarks>
/// Providers run in the order their features depend on each other, so the owner of a list adds its columns
/// before the modules depending on it run. A module that changes or removes a column of another module's list
/// depends on the feature owning it. Adding a column does not depend on that order: the columns are sorted by
/// <see cref="Models.AdminListColumn.Position"/> once every provider ran.
/// </remarks>
public interface IAdminListColumnProvider
{
    /// <summary>
    /// Alters the columns of the list identified by <see cref="AdminListColumnsContext.ListName"/>.
    /// </summary>
    /// <param name="context">The columns of the list being rendered.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    Task BuildAsync(AdminListColumnsContext context, CancellationToken cancellationToken = default);
}
