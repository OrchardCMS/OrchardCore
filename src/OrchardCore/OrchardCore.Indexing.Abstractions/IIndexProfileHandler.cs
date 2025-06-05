using OrchardCore.Indexing.Models;
using OrchardCore.Infrastructure.Entities;

namespace OrchardCore.Indexing;

public interface IIndexProfileHandler : IModelHandler<IndexProfile>
{
    /// <summary>
    /// Invoked when an <see cref="IndexProfile"/> has been synchronized.
    /// </summary>
    /// <param name="context">The context containing the synchronized <see cref="IndexProfile"/>.</param>
    Task SynchronizedAsync(IndexProfileSynchronizedContext context);

    /// <summary>
    /// Invoked when an <see cref="IndexProfile"/> needs to be reset.
    /// </summary>
    /// <param name="context">The context containing the <see cref="IndexProfile"/> to reset.</param>
    Task ResetAsync(IndexProfileResetContext context);

    /// <summary>
    /// Invoked when an <see cref="IndexProfile"/> is being exported.
    /// </summary>
    /// <param name="context">The context containing the <see cref="IndexProfile"/> to export.</param>
    Task ExportingAsync(IndexProfileExportingContext context);
}
