using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin;

/// <summary>
/// Creates the <c>AdminList</c> shape of a list, with its columns and the layout to render it with.
/// </summary>
public interface IAdminListFactory
{
    /// <summary>
    /// Creates the <c>AdminList</c> shape of the given list. The columns are built with
    /// <see cref="IAdminListColumnsBuilder"/>, and the layout is resolved with <see cref="IAdminListLayoutResolver"/>
    /// unless <see cref="AdminListContext.Layout"/> is set. A list no provider declared columns for is rendered with
    /// the <see cref="AdminListConstants.List"/> layout, the only one that does not need them.
    /// </summary>
    /// <param name="context">The list to render.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    Task<IShape> CreateAsync(AdminListContext context, CancellationToken cancellationToken = default);
}
