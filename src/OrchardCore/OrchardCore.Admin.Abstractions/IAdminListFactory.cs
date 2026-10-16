using OrchardCore.DisplayManagement;

namespace OrchardCore.Admin;

/// <summary>
/// Creates the <c>AdminList</c> shape of a list, with its columns and the layout to render it with.
/// </summary>
public interface IAdminListFactory
{
    /// <summary>
    /// Creates an <c>AdminList</c> shape. The rendering view can set its columns and presentation properties
    /// before displaying it. Initialization is deferred until display so standard shape creation works too.
    /// </summary>
    /// <param name="name">The list identity used by column providers, layout preferences and shape alternates.</param>
    /// <param name="context">The items, contextual data and region shapes to render.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/> to observe.</param>
    Task<IShape> CreateAsync(string name, AdminListContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares an <c>AdminList</c> shape for display: extends its columns, resolves its layout, builds its
    /// rows using the selected display type and its toolbar. Called by the shape's displaying event.
    /// </summary>
    /// <param name="shape">The shape created through this factory or the standard shape factory.</param>
    /// <param name="cancellationToken">The cancellation token to observe.</param>
    Task BuildAsync(IShape shape, CancellationToken cancellationToken = default);
}
