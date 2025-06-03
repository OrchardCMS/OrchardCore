using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing;

public interface IIndexEntityHandler : IModelHandler<IndexEntity>
{
    /// <summary>
    /// Invoked when an <see cref="IndexEntity"/> has been synchronized.
    /// </summary>
    /// <param name="context">The context containing the synchronized <see cref="IndexEntity"/>.</param>
    Task SynchronizedAsync(IndexEntitySynchronizedContext context);

    /// <summary>
    /// Invoked when an <see cref="IndexEntity"/> needs to be reset.
    /// </summary>
    /// <param name="context">The context containing the <see cref="IndexEntity"/> to reset.</param>
    Task ResetAsync(IndexEntityResetContext context);

    /// <summary>
    /// Invoked when an <see cref="IndexEntity"/> is being exported.
    /// </summary>
    /// <param name="context">The context containing the <see cref="IndexEntity"/> to export.</param>
    Task ExportingAsync(IndexEntityExportingContext context);
}
