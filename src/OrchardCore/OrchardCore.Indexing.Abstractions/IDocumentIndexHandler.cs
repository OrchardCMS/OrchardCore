using OrchardCore.Indexing.Models;

namespace OrchardCore.Indexing;

/// <summary>
/// An implementation of <see cref="IDocumentIndexHandler"/> can provide property values for an index document.
/// </summary>
public interface IDocumentIndexHandler
{
    /// <summary>
    /// Builds index entries for the current record by adding values to <see cref="BuildDocumentIndexContext.DocumentIndex"/>.
    /// </summary>
    /// <param name="context">
    /// The context for the current indexing operation, including the source record, target document,
    /// generated field keys, and provider-specific index settings.
    /// </param>
    Task BuildIndexAsync(BuildDocumentIndexContext context);

    /// <summary>
    /// Notifies the handler after documents have been added to or updated in a provider-specific index.
    /// </summary>
    /// <param name="indexProfile">The index profile that was updated.</param>
    /// <param name="documentIds">The distinct identifiers of the documents that were successfully added or updated.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the notification.</param>
    Task DocumentsAddedOrUpdatedAsync(
        IndexProfile indexProfile,
        IEnumerable<string> documentIds,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;

    /// <summary>
    /// Notifies the handler after documents have been removed from a provider-specific index.
    /// </summary>
    /// <param name="indexProfile">The index profile that was updated.</param>
    /// <param name="documentIds">The distinct identifiers of the documents that were successfully deleted.</param>
    /// <param name="cancellationToken">A token that can be used to cancel the notification.</param>
    Task DocumentsDeletedAsync(
        IndexProfile indexProfile,
        IEnumerable<string> documentIds,
        CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
